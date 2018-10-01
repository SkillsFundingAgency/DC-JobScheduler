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
    public sealed class EsfMessageFactory : AbstractFileUploadMessageFactory
    {
        private readonly EsfMessageTopics _esfMessageTopics;
        private readonly IFileUploadJobManager _fileUploadJobManager;
        private readonly ITopicConfiguration _topicConfiguration;

        public EsfMessageFactory(
            EsfMessageTopics esfMessageTopics,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.EsfSubmission)]ITopicConfiguration topicConfiguration)
            : base(logger, fileUploadMetaDataManager, topicConfiguration)
        {
            _esfMessageTopics = esfMessageTopics;
            _fileUploadJobManager = fileUploadMetaDataManager;
            _topicConfiguration = topicConfiguration;
        }

        public override void AddExtraKeys(JobContextMessage message, FileUploadJob metaData)
        {
        }

        public override List<TopicItem> CreateTopics(bool isFirstStage)
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
