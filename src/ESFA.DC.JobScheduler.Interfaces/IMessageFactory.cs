using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.JobScheduler.Interfaces.Models;

namespace ESFA.DC.JobScheduler.Interfaces
{
    public interface IMessageFactory
    {
        MessageParameters CreateMessageParameters(long jobId, bool isCrossLoaded);
    }
}
