using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class IlrSecondStageMessageTopics
    {
        [JsonRequired]
        public string TopicValidation { get; set; }

        [JsonRequired]
        public string TopicFunding { get; set; }

        [JsonRequired]
        public string TopicDeds { get; set; }

        [JsonRequired]
        public string TopicDeds_TaskPersistDataToDeds { get; set; }

        [JsonRequired]
        public string TopicReports { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateValidationReport { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateAllbOccupancyReport { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateFundingSummaryReport { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateMainOccupancyReport { get; set; }

        [JsonRequired]
        public string TopicFunding_TaskPerformALBCalculation { get; set; }

        [JsonRequired]
        public string TopicFunding_TaskPerformFM25Calculation { get; set; }

        [JsonRequired]
        public string TopicFunding_TaskPerformFM35Calculation { get; set; }

        [JsonRequired]
        public string TopicFunding_TaskPerformFM36Calculation { get; set; }

        [JsonRequired]
        public string TopicFunding_TaskPerformFM70Calculation { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateDataMatchReport { get; set; }
    }
}
