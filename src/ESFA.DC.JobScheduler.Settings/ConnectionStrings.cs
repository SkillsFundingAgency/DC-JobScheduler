using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class ConnectionStrings
    {
        [JsonRequired]
        public string AppLogs { get; set; }

        [JsonRequired]
        public string Organisation { get; set; }
    }
}
