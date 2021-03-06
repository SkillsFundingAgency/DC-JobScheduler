﻿using Autofac.Features.AttributeFilters;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler
{
    public sealed class ReferenceDataMessageFactory : AbstractFileUploadMessageFactory
    {
        public ReferenceDataMessageFactory(
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.ReferenceDataFCS)]
            ITopicConfiguration topicConfiguration,
            IJobTopicTaskService jobTopicTaskService)
            : base(logger, fileUploadMetaDataManager, topicConfiguration, jobTopicTaskService)
        {
        }

        public override void AddExtraKeys(IJobContextMessage message, FileUploadJob metaData)
        {
        }
    }
}
