using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Models;
using ESFA.DC.JobQueueManager.Models.Enums;
using ESFA.DC.JobScheduler.ServiceBus;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public class QueueHandler : IQueueHandler
    {
        private readonly IJobSchedulerStatusManager _jobSchedulerStatusManager;
        private readonly IMessagingService _messagingService;
        private readonly IJobQueueManager _jobQueueManager;
        private readonly IAuditor _auditor;

        public QueueHandler(IMessagingService messagingService, IJobQueueManager jobQueueManager, IAuditor auditor, IJobSchedulerStatusManager jobSchedulerStatusManager)
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
                       await MoveJobForProcessing(job);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public async Task MoveJobForProcessing(Job job)
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
            var topics = new List<ITopicItem>()
            {
                new TopicItem("fundingcalc-init", "fundingcalc-init", tasks)
            };

            var message = new JobContextMessage(job.JobId, topics, job.Ukprn.ToString(), job.StorageReference, job.FileName, null, 0, job.DateTimeSubmittedUtc);
            try
            {
                var jobStatusUpdated = _jobQueueManager.UpdateJobStatus(job.JobId, JobStatus.MovedForProcessing);

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

                        _jobQueueManager.UpdateJobStatus(job.JobId, JobStatus.Failed);
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
    }
}