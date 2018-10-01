using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Interfaces.Models;

namespace ESFA.DC.JobScheduler.Interfaces
{
    public interface IMessagingService
    {
        Task SendMessageAsync(MessageParameters messageParameters);
   }
}