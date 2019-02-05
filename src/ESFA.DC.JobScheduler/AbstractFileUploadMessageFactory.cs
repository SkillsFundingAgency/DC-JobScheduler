using System.Collections.Generic;
using System.Linq;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Interfaces.Models;
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
            FileUploadJob job = _fileUploadJobManager.GetJobById(jobId);

            List<ITopicItem> topics = _jobTopicTaskService.GetTopicItems(job.JobType, job.IsFirstStage).ToList();

            JobContextMessage contextMessage = new JobContextMessage(
                job.JobId,
                topics,
                job.Ukprn.ToString(),
                job.StorageReference,
                job.FileName,
                job.SubmittedBy,
                0,
                job.DateTimeSubmittedUtc);

            contextMessage.KeyValuePairs.Add("CollectionName", job.CollectionName);
            contextMessage.KeyValuePairs.Add("ReturnPeriod", job.PeriodNumber);

            AddExtraKeys(contextMessage, job);

            MessageParameters message = new MessageParameters(job.JobType)
            {
                JobContextMessage = contextMessage,
                SubscriptionLabel = topics[0].SubscriptionName,
                TopicParameters = new Dictionary<string, object>
                {
                    {
                        "To", topics[0].SubscriptionName
                    }
                }
            };

            return message;
        }

        public abstract void AddExtraKeys(IJobContextMessage message, FileUploadJob metaData);

        protected string GenerateKey(long ukprn, long jobId, string value, string extension = null)
        {
            string key = $"{ukprn}/{jobId}/{value}";
            if (!string.IsNullOrEmpty(extension))
            {
                if (!extension.StartsWith("."))
                {
                    key += ".";
                }

                key += extension;
            }

            return key;
        }
    }
}
