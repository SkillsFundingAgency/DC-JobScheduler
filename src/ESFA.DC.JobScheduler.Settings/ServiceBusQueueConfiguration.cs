using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler.Settings
{
    public class ServiceBusQueueConfiguration : IQueueConfiguration
    {
        public string QueueName { get; set; }

        public string ConnectionString { get; set; }

        public int MaxConcurrentCalls => 1;

        public int MinimumBackoffSeconds => 2;

        public int MaximumBackoffSeconds => 5;

        public int MaximumRetryCount => 3;

        public TimeSpan MaximumCallbackTimeSpan => new TimeSpan(0, 10, 0);
    }
}
