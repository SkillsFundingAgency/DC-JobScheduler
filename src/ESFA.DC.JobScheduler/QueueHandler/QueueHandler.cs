using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.JobStatus.Interface;
using Remotion.Linq.Clauses;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public class QueueHandler : IQueueHandler
    {
        private readonly IJobSchedulerStatusManager _jobSchedulerStatusManager;
        private readonly IMessagingService _messagingService;
        private readonly IIlrJobQueueManager _jobQueueManager;
        private readonly IAuditor _auditor;

        public QueueHandler(IMessagingService messagingService, IIlrJobQueueManager jobQueueManager, IAuditor auditor, IJobSchedulerStatusManager jobSchedulerStatusManager)
        {
            _jobSchedulerStatusManager = jobSchedulerStatusManager;
            _jobQueueManager = jobQueueManager;
            _messagingService = messagingService;
            _auditor = auditor;
        }

        public async Task ProcessNextJobAsync()
        {
            while (true)
            {
                if (await _jobSchedulerStatusManager.IsJobQueueProcessingEnabledAsync())
                {
                    var job = _jobQueueManager.GetJobByPriority();

                    if (job != null)
                    {
                        switch (job.JobType)
                        {
                            case JobType.IlrSubmission:
                                await MoveIlrJobForProcessing(job);
                                break;
                            case JobType.ReferenceData:
                                throw new NotImplementedException();
                            case JobType.PeriodEnd:
                                throw new NotImplementedException();
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public async Task MoveIlrJobForProcessing(IlrJob job)
        {
            if (job == null)
            {
                return;
            }

            var tasks = new List<ITaskItem>()
            {
                new TaskItem()
                {
                    Tasks = new List<string>() { string.Empty },
                    SupportsParallelExecution = false
                }
            };
            var topics = new List<TopicItem>()
            {
                new TopicItem("validation", "validation", tasks),
                new TopicItem("FundingCalc", "FundingCalc", tasks),
                new TopicItem("data-store", "data-store", tasks),
            };

            var message = new JobContextMessage(
                job.JobId,
                topics,
                job.Ukprn.ToString(),
                job.StorageReference,
                job.FileName,
                job.SubmittedBy,
                0,
                job.DateTimeSubmittedUtc);

            message.KeyValuePairs.Add(JobContextMessageKey.FileSizeInBytes, job.FileSize);

            try
            {
                var jobStatusUpdated = _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing);

                if (jobStatusUpdated)
                {
                    try
                    {
                        await _messagingService.SendMessagesAsync(message);
                        await _auditor.AuditAsync(message, AuditEventType.JobSubmitted);
                    }
                    catch (Exception ex)
                    {
                        await _auditor.AuditAsync(message, AuditEventType.ServiceFailed, $"Failed to send message to Servie bus queue with exception : {ex}");

                        _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.Failed);
                    }
                }
                else
                {
                    await _auditor.AuditAsync(message, AuditEventType.JobFailed, "Failed to update job status, no message is added to the service bus queue");
                }
            }
            catch (Exception exception)
            {
                await _auditor.AuditAsync(message, AuditEventType.ServiceFailed, $"Failed to update job status with exception : {exception}");
            }
        }

        public Task MoveReferenceJobForProcessing(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}