using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class IlrFirstStageMessageTopics
    {
        [JsonRequired]
        public string TopicFileValidation { get; set; }

        [JsonRequired]
        public string TopicValidation { get; set; }

        [JsonRequired]
        public string TopicFunding { get; set; }

        [JsonRequired]
        public string TopicFunding_TaskPerformFM36Calculation { get; set; }

        [JsonRequired]
        public string TopicReports { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateValidationReport { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateDataMatchReport { get; set; }
    }
}
