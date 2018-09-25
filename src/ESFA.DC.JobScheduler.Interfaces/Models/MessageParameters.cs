using System.Collections.Generic;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model.Enums;

namespace ESFA.DC.JobScheduler.Interfaces.Models
{
    public class MessageParameters
    {
        public MessageParameters(JobType jobType)
        {
            JobType = jobType;
            JobContextMessage = new JobContextMessage();
        }

        public IJobContextMessage JobContextMessage { get; set; }

        public string SubscriptionLabel { get; set; }

        public IDictionary<string, object> TopicParameters { get; set; }

        public JobType JobType { get; set; }

        public bool IsCrossLoaded { get; set; }
    }
}
