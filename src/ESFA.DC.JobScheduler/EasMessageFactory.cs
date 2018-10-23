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
    public sealed class EasMessageFactory : AbstractFileUploadMessageFactory
    {
        private readonly EasMessageTopics _messageTopics;

        public EasMessageFactory(
            EasMessageTopics messageTopics,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.EasSubmission)]ITopicConfiguration topicConfiguration)
            : base(logger, fileUploadMetaDataManager, topicConfiguration)
        {
            _messageTopics = messageTopics;
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

            topics.Add(new TopicItem(_messageTopics.TopicProcessing, _messageTopics.TopicProcessing, tasks));
            return topics;
        }
    }
}
