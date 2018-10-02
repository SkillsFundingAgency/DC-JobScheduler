using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ESFA.DC.JobScheduler.Settings
{
    public class EsfMessageTopics
    {
        [JsonRequired]
        public string TopicProcessing { get; set; }
    }
}
