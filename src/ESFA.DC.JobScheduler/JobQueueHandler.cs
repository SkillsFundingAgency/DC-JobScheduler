using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.JobScheduler
{
    public class JobQueueHandler : IJobQueueHandler
    {
        private readonly IJobSchedulerStatusManager _jobSchedulerStatusManager;
        private readonly IMessagingService _messagingService;
        private readonly IJobManager _jobQueueManager;
        private readonly IAuditor _auditor;
        private readonly IIndex<JobType, IMessageFactory> _jobContextMessageFactories;
        private readonly ILogger _logger;

        public JobQueueHandler(
            IMessagingService messagingService,
            IJobManager jobQueueManager,
            IAuditor auditor,
            IJobSchedulerStatusManager jobSchedulerStatusManager,
            IIndex<JobType, IMessageFactory> jobContextMessageFactories,
            ILogger logger)
        {
            _jobSchedulerStatusManager = jobSchedulerStatusManager;
            _jobQueueManager = jobQueueManager;
            _messagingService = messagingService;
            _auditor = auditor;
            _jobContextMessageFactories = jobContextMessageFactories;
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
                            await MoveJobForProcessing(job);
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

        public async Task MoveJobForProcessing(Jobs.Model.Job job)
        {
            if (job == null)
            {
                return;
            }

            _logger.LogInfo($"Job id : {job.JobId} recieved for moving to queue");

            var message = _jobContextMessageFactories[job.JobType].CreateMessageParameters(job.JobId, job.IsCrossLoaded);

            try
            {
                var jobStatusUpdated = _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing);

                _logger.LogInfo($"Job id : {job.JobId} status updated successfully");

                if (jobStatusUpdated)
                {
                    try
                    {
                        await _messagingService.SendMessageAsync(message);
                        await _auditor.AuditAsync(message.JobContextMessage, AuditEventType.JobSubmitted);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Job id : {job.JobId} sending to service bus failed", ex);
                        await _auditor.AuditAsync(message.JobContextMessage, AuditEventType.ServiceFailed, $"Failed to send message to Servie bus queue with exception : {ex}");
                        _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.Failed);
                    }
                }
                else
                {
                    _logger.LogWarning($"Job id : {job.JobId} failed to send to service bus");
                    await _auditor.AuditAsync(message.JobContextMessage, AuditEventType.JobFailed, "Failed to update job status, no message is added to the service bus queue");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Job id : {job.JobId}", exception);
                await _auditor.AuditAsync(message.JobContextMessage, AuditEventType.ServiceFailed, $"Failed to update job status with exception : {exception}");
            }
        }
    }
}