using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class EasMessageTopics
    {
        [JsonRequired]
        public string TopicProcessing { get; set; }

        [JsonRequired]
        public string TopicProcessing_TaskValidation { get; set; }

        [JsonRequired]
        public string TopicProcessing_TaskStorage { get; set; }

        [JsonRequired]
        public string TopicProcessing_TaskReporting { get; set; }
    }
}
