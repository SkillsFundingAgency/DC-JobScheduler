using System.Threading.Tasks;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public interface IQueueHandler
    {
        Task ProcessNextJobAsync();

        Task MoveFileUploadJobForProcessing(Jobs.Model.Job job);

        Task MoveReferenceJobForProcessing(Jobs.Model.Job job);
    }
}
