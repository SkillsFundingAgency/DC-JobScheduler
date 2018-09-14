using System.Collections.Generic;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model.Enums;

namespace ESFA.DC.JobScheduler.Interfaces.Models
{
    public class MessageParameters
    {
        public MessageParameters(JobType jobType)
        {
            JobType = jobType;
        }

        public IJobContextMessage JobContextMessage { get; set; }

        public string SubscriptionLabel { get; set; }

        public IDictionary<string, object> TopicParameters { get; set; }

        public JobType JobType { get; set; }
    }
}
