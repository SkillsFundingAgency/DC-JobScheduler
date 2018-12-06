using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public abstract class AbstractFileUploadMessageFactory : IMessageFactory
    {
        private readonly ILogger _logger;
        private readonly IFileUploadJobManager _fileUploadJobManager;
        private readonly ITopicConfiguration _topicConfiguration;
        private readonly IJobTopicTaskService _jobTopicTaskService;

        protected AbstractFileUploadMessageFactory(
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            ITopicConfiguration topicConfiguration,
            IJobTopicTaskService jobTopicTaskService)
        {
            _logger = logger;
            _fileUploadJobManager = fileUploadMetaDataManager;
            _topicConfiguration = topicConfiguration;
            _jobTopicTaskService = jobTopicTaskService;
        }

        public MessageParameters CreateMessageParameters(long jobId)
        {
            var job = _fileUploadJobManager.GetJobById(jobId);

            var topics = CreateTopics(job.JobType, job.IsFirstStage);

            var contextMessage = new JobContextMessage(
                job.JobId,
                topics,
                job.Ukprn.ToString(),
                job.StorageReference,
                job.FileName,
                job.SubmittedBy,
                0,
                job.DateTimeSubmittedUtc);

            AddExtraKeys(contextMessage, job);

            var message = new MessageParameters(job.JobType)
            {
                JobContextMessage = contextMessage,
                TopicParameters = new Dictionary<string, object>
                {
                    {
                        "To", _topicConfiguration.SubscriptionName
                    }
                },
                SubscriptionLabel = _topicConfiguration.SubscriptionName,
            };

            return message;
        }

        public virtual void AddExtraKeys(JobContextMessage message, FileUploadJob metaData)
        {
        }

        public List<ITopicItem> CreateTopics(JobType jobType, bool isFirstStage)
        {
            var topics = _jobTopicTaskService.GetTopicItems(jobType, isFirstStage);
            return topics.ToList();
        }
    }
}
