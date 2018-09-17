using System.Collections.Generic;
using Autofac.Features.AttributeFilters;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Interfaces.Models;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler
{
    public sealed class EsfMessageFactory : IMessageFactory
    {
        private readonly EsfMessageTopics _esfMessageTopics;
        private readonly IKeyGenerator _keyGenerator;
        private readonly IFileUploadJobManager _fileUploadJobManager;
        private readonly ITopicConfiguration _topicConfiguration;

        public EsfMessageFactory(
            EsfMessageTopics esfMessageTopics,
            IKeyGenerator keyGenerator,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.EsfSubmission)]ITopicConfiguration topicConfiguration)
        {
            _esfMessageTopics = esfMessageTopics;
            _keyGenerator = keyGenerator;
            _fileUploadJobManager = fileUploadMetaDataManager;
            _topicConfiguration = topicConfiguration;
        }

        public MessageParameters CreateMessageParameters(long jobId)
        {
            var job = _fileUploadJobManager.GetJobById(jobId);

            var topics = CreateTopicsList();

            var contextMessage = new JobContextMessage(
                job.JobId,
                topics,
                job.Ukprn.ToString(),
                job.StorageReference,
                job.FileName,
                job.SubmittedBy,
                0,
                job.DateTimeSubmittedUtc);

            var message = new MessageParameters(JobType.EsfSubmission)
            {
                JobContextMessage = contextMessage,
                TopicParameters = new Dictionary<string, object>
                {
                    {
                        "To", _topicConfiguration.SubscriptionName
                    }
                },
                SubscriptionLabel = _topicConfiguration.SubscriptionName
            };

            return message;
        }

        public List<TopicItem> CreateTopicsList()
        {
            var topics = new List<TopicItem>();

            var tasks = new List<ITaskItem>()
            {
                new TaskItem()
                {
                    Tasks = new List<string>() { string.Empty },
                    SupportsParallelExecution = false
                }
            };

            topics.Add(new TopicItem(_esfMessageTopics.TopicValidation, _esfMessageTopics.TopicValidation, tasks));
            topics.Add(new TopicItem(_esfMessageTopics.TopicFunding, _esfMessageTopics.TopicFunding, tasks));
            topics.Add(new TopicItem(_esfMessageTopics.TopicReports, _esfMessageTopics.TopicReports, tasks));

            return topics;
        }
    }
}
