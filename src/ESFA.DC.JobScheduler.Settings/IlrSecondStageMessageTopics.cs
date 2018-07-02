using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class IlrSecondStageMessageTopics
    {
        [JsonRequired]
        public string TopicValidation { get; set; }

        [JsonRequired]
        public string TopicDeds_TaskGenerateValidationReport { get; set; }

        [JsonRequired]
        public string TopicFunding { get; set; }

        [JsonRequired]
        public string TopicDeds { get; set; }

        [JsonRequired]
        public string TopicReports { get; set; }

        [JsonRequired]
        public string TopicDeds_TaskPersistDataToDeds { get; set; }
    }
}
