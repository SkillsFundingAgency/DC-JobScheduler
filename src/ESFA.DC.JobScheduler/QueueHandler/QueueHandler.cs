using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.Job.Models.Enums;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.JobContextMessage;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.JobScheduler.QueueHandler
{
    public class QueueHandler : IQueueHandler
    {
        private readonly IJobSchedulerStatusManager _jobSchedulerStatusManager;
        private readonly IMessagingService _messagingService;
        private readonly IJobManager _jobQueueManager;
        private readonly IAuditor _auditor;
        private readonly JobContextMessageFactory _jobContextMessageFactory;
        private readonly ILogger _logger;

        public QueueHandler(
            IMessagingService messagingService,
            IJobManager jobQueueManager,
            IAuditor auditor,
            IJobSchedulerStatusManager jobSchedulerStatusManager,
            JobContextMessageFactory jobContextMessageFactory,
            ILogger logger)
        {
            _jobSchedulerStatusManager = jobSchedulerStatusManager;
            _jobQueueManager = jobQueueManager;
            _messagingService = messagingService;
            _auditor = auditor;
            _jobContextMessageFactory = jobContextMessageFactory;
            _logger = logger;
        }

        public async Task ProcessNextJobAsync()
        {
            while (true)
            {
                try
                {
                    if (await _jobSchedulerStatusManager.IsJobQueueProcessingEnabledAsync())
                    {
                        var job = _jobQueueManager.GetJobByPriority();

                        if (job != null)
                        {
                            _logger.LogInfo($"Got job id : {job.JobId}");

                            switch (job.JobType)
                            {
                                case JobType.IlrSubmission:
                                    await MoveFileUploadJobForProcessing(job);
                                    break;

                                case JobType.ReferenceData:
                                    throw new NotImplementedException();
                                case JobType.PeriodEnd:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured in job scheduler - will continue to pick new jobs", ex);
                }

                Thread.Sleep(1000);
            }
        }

        public async Task MoveFileUploadJobForProcessing(Job.Models.Job job)
        {
            if (job == null)
            {
                return;
            }

            _logger.LogInfo($"Job id : {job.JobId} recieved for moving to queue");

            var message = _jobContextMessageFactory.CreateFileUploadJobContextMessage(job);

            try
            {
                var jobStatusUpdated = _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing);

                _logger.LogInfo($"Job id : {job.JobId} status updated successfully");

                if (jobStatusUpdated)
                {
                    try
                    {
                        await _messagingService.SendMessagesAsync(message);
                        await _auditor.AuditAsync(message, AuditEventType.JobSubmitted);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Job id : {job.JobId} sending to service bus failed", ex);
                        await _auditor.AuditAsync(message, AuditEventType.ServiceFailed, $"Failed to send message to Servie bus queue with exception : {ex}");
                        _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.Failed);
                    }
                }
                else
                {
                    _logger.LogWarning($"Job id : {job.JobId} failed to send to service bus");
                    await _auditor.AuditAsync(message, AuditEventType.JobFailed, "Failed to update job status, no message is added to the service bus queue");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Job id : {job.JobId}", exception);
                await _auditor.AuditAsync(message, AuditEventType.ServiceFailed, $"Failed to update job status with exception : {exception}");
            }
        }

        public Task MoveReferenceJobForProcessing(Job.Models.Job job)
        {
            throw new NotImplementedException();
        }
    }
}