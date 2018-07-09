using System.Threading.Tasks;
using ESFA.DC.JobContext;

namespace ESFA.DC.JobScheduler.ServiceBus
{
    public interface IMessagingService
    {
        Task SendMessagesAsync(JobContext.JobContextMessage message);
    }
}