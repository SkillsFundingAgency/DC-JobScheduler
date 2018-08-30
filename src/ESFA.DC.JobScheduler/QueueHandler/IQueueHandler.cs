using System.Threading.Tasks;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public interface IQueueHandler
    {
        Task ProcessNextJobAsync();

        Task MoveIlrJobForProcessing(IlrJob job);

        Task MoveReferenceJobForProcessing(IJob job);
    }
}
