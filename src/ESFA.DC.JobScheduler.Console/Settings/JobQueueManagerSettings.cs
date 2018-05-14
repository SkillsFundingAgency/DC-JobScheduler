using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.JobQueueManager.Interfaces;
using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Console.Settings
{
    public class JobQueueManagerSettings
    {
        [JsonRequired]
        public string ConnectionString { get; set; }
    }
}
