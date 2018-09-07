using System.Threading.Tasks;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public interface IQueueHandler
    {
        Task ProcessNextJobAsync();

        Task MoveFileUploadJobForProcessing(Job.Models.Job job);

        Task MoveReferenceJobForProcessing(Job.Models.Job job);
    }
}
