using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class IlrFirstStageMessageTopics
    {
        [JsonRequired]
        public string TopicValidation { get; set; }

        [JsonRequired]
        public string TopicDeds_TaskGenerateValidationReport { get; set; }
    }
}
