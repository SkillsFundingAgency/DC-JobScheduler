using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobScheduler.ServiceBus
{
    public class QueueConfiguration : IQueueConfiguration
    {
        public string ConnectionString => string.Empty;

        public string QueueName => string.Empty;

        public string TopicName => string.Empty;

        public int MaxConcurrentCalls => 1;

        public int MinimumBackoffSeconds => 2;

        public int MaximumBackoffSeconds => 5;

        public int MaximumRetryCount => 3;
    }
}
