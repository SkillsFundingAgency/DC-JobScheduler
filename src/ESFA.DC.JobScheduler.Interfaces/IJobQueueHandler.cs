using System.Threading.Tasks;

namespace ESFA.DC.JobScheduler.Interfaces
{
    public interface IJobQueueHandler
    {
        Task ProcessNextJobAsync();

        Task MoveJobForProcessing(Jobs.Model.Job job);
    }
}
