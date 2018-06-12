using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Models;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public interface IQueueHandler
    {
        Task ProcessNextJobAsync();

        Task MoveJobForProcessing(Job job);
    }
}
