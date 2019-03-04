using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.JobScheduler.Interfaces
{
    public interface IJobQueueHandler
    {
        Task ProcessNextJobAsync(CancellationToken cancellationToken);

        Task MoveJobForProcessingAsync(Jobs.Model.Job job);
    }
}
