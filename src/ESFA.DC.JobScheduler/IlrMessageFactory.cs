using System.Collections.Generic;
using System.Linq;
using Autofac.Features.AttributeFilters;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler
{
    public sealed class IlrMessageFactory : AbstractFileUploadMessageFactory
    {
        private readonly IKeyGenerator _keyGenerator;

        public IlrMessageFactory(
            IKeyGenerator keyGenerator,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.IlrSubmission)]ITopicConfiguration topicConfiguration,
            IJobTopicTaskService jobTopicTaskService)
            : base(logger, fileUploadMetaDataManager, topicConfiguration, jobTopicTaskService)
        {
            _keyGenerator = keyGenerator;
        }

        public override void AddExtraKeys(JobContextMessage message, FileUploadJob metaData)
        {
            if (message.KeyValuePairs == null)
            {
                message.KeyValuePairs = new Dictionary<string, object>();
            }

            message.KeyValuePairs.Add(JobContextMessageKey.FileSizeInBytes, metaData.FileSize);

            if (metaData.IsFirstStage)
            {
                message.KeyValuePairs.Add(JobContextMessageKey.PauseWhenFinished, "1");
            }

            if (metaData.Ukprn == 0)
            {
                return;
            }

            message.KeyValuePairs.Add(JobContextMessageKey.InvalidLearnRefNumbers, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationInvalidLearners));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidLearnRefNumbers, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationValidLearners));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrors, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationErrors));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrorLookups, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationErrorsLookup));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingAlbOutput, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.FundingAlbOutput));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm35Output, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.FundingFm35Output));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm25Output, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.FundingFm25Output));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm36Output, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, JobContextMessageKey.FundingFm36Output));
            message.KeyValuePairs.Add("FundingFm70Output", _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, "FundingFm70Output"));
            message.KeyValuePairs.Add("FundingFm81Output", _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, "FundingFm81Output"));
            message.KeyValuePairs.Add("OriginalFilename", metaData.FileName);
            message.KeyValuePairs.Add("CollectionYear", metaData.CollectionYear);
        }
    }
}
