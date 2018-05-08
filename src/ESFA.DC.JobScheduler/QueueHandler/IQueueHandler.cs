using System.Threading.Tasks;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public interface IQueueHandler
    {
        Task ProcessNextJobAsync();
    }
}
