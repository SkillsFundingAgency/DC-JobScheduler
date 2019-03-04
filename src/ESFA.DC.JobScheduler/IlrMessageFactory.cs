using Autofac.Features.AttributeFilters;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler
{
    public sealed class IlrMessageFactory : AbstractFileUploadMessageFactory
    {
        public IlrMessageFactory(
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.IlrSubmission)]
            ITopicConfiguration topicConfiguration,
            IJobTopicTaskService jobTopicTaskService)
            : base(logger, fileUploadMetaDataManager, topicConfiguration, jobTopicTaskService)
        {
        }

        public override void AddExtraKeys(IJobContextMessage message, FileUploadJob metaData)
        {
            message.KeyValuePairs.Add(JobContextMessageKey.FileSizeInBytes, metaData.FileSize);

            if (metaData.IsFirstStage)
            {
                message.KeyValuePairs.Add(JobContextMessageKey.PauseWhenFinished, "1");
            }

            if (metaData.Ukprn == 0)
            {
                return;
            }

            message.KeyValuePairs.Add(JobContextMessageKey.InvalidLearnRefNumbers, GenerateKey(metaData.Ukprn, metaData.JobId, "ValidationInvalidLearners", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidLearnRefNumbers, GenerateKey(metaData.Ukprn, metaData.JobId, "ValidationValidLearners", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrors, GenerateKey(metaData.Ukprn, metaData.JobId, "ValidationErrors", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrorLookups, GenerateKey(metaData.Ukprn, metaData.JobId, "ValidationErrorsLookup", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingAlbOutput, GenerateKey(metaData.Ukprn, metaData.JobId, "FundingAlbOutput", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm35Output, GenerateKey(metaData.Ukprn, metaData.JobId, "FundingFm35Output", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm25Output, GenerateKey(metaData.Ukprn, metaData.JobId, "FundingFm25Output", "json"));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm36Output, GenerateKey(metaData.Ukprn, metaData.JobId, JobContextMessageKey.FundingFm36Output, "json"));
            message.KeyValuePairs.Add("FundingFm70Output", GenerateKey(metaData.Ukprn, metaData.JobId, "FundingFm70Output", "json"));
            message.KeyValuePairs.Add("FundingFm81Output", GenerateKey(metaData.Ukprn, metaData.JobId, "FundingFm81Output", "json"));
            message.KeyValuePairs.Add("OriginalFilename", metaData.FileName);
            message.KeyValuePairs.Add("CollectionYear", metaData.CollectionYear);
        }
    }
}
