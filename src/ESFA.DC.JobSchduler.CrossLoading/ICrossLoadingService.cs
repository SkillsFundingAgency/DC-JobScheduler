using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobSchduler.CrossLoading
{
    public interface ICrossLoadingService
    {
        Task<bool> SendMessageForCrossLoadingAsync(long jobId);
    }
}
