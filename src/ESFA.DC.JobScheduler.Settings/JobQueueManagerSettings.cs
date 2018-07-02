using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class JobQueueManagerSettings
    {
        [JsonRequired]
        public string ConnectionString { get; set; }
    }
}
