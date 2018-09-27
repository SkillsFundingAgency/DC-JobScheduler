﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class EsfMessageTopics
    {
        [JsonRequired]
        public string TopicValidation { get; set; }

        [JsonRequired]
        public string TopicFunding { get; set; }

        [JsonRequired]
        public string TopicReports { get; set; }

        [JsonRequired]
        public string TopicReports_TaskGenerateValidationReport { get; set; }
    }
}