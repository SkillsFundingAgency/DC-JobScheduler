using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.JobScheduler.Interfaces.Models;

namespace ESFA.DC.JobScheduler.Interfaces
{
    public interface IMessageFactory
    {
        Task<MessageParameters> CreateMessageParametersAsync(long jobId);
    }
}
