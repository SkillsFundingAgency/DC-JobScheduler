using Autofac.Features.AttributeFilters;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler
{
    public sealed class EasMessageFactory : AbstractFileUploadMessageFactory
    {
        public EasMessageFactory(
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.EasSubmission)]ITopicConfiguration topicConfiguration,
            IJobTopicTaskService jobTopicTaskService)
            : base(logger, fileUploadMetaDataManager, topicConfiguration, jobTopicTaskService)
        {
        }

        public override void AddExtraKeys(IJobContextMessage message, FileUploadJob metaData)
        {
        }
    }
}
