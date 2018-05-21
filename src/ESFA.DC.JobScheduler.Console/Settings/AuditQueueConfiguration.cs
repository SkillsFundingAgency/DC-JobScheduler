using ESFA.DC.Queueing.Interface;
using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Console.Settings
{
    public class AuditQueueConfiguration : IQueueConfiguration
    {
        [JsonRequired]
        public string ConnectionString { get; set; }

        [JsonRequired]
        public string QueueName { get; set; }

        public string TopicName => string.Empty;

        public int MaxConcurrentCalls => 1;

        public int MinimumBackoffSeconds => 2;

        public int MaximumBackoffSeconds => 5;

        public int MaximumRetryCount => 3;
    }
}
