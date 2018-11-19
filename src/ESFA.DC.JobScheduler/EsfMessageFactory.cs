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

        public EsfMessageFactory(
            EsfMessageTopics esfMessageTopics,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.EsfSubmission)]ITopicConfiguration topicConfiguration)
            : base(logger, fileUploadMetaDataManager, topicConfiguration)
        {
            _esfMessageTopics = esfMessageTopics;
        }

        public override void AddExtraKeys(JobContextMessage message, FileUploadJob metaData)
        {
            message.KeyValuePairs[JobContextMessageKey.UkPrn] = metaData.Ukprn;
        }

        public override List<TopicItem> CreateTopics(bool isFirstStage)
        {
            var topics = new List<TopicItem>();

            var tasks = new List<ITaskItem>()
            {
                new TaskItem()
                {
                    Tasks = new List<string>()
                    {
                        _esfMessageTopics.TopicProcessing_TaskValidation,
                        _esfMessageTopics.TopicProcessing_TaskStorage,
                        _esfMessageTopics.TopicProcessing_TaskReporting
                    },
                    SupportsParallelExecution = false
                }
            };

            topics.Add(new TopicItem(_esfMessageTopics.TopicProcessing, _esfMessageTopics.TopicProcessing, tasks));
            return topics;
        }
    }
}
