﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Interfaces.ExternalData;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobSchduler.CrossLoading;
using ESFA.DC.JobScheduler.Interfaces;
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
        private readonly ICrossLoadingService _crossLoadingService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IExternalDataScheduleService _externalDataScheduleService;

        public JobQueueHandler(
            IMessagingService messagingService,
            IJobManager jobQueueManager,
            IAuditor auditor,
            IJobSchedulerStatusManager jobSchedulerStatusManager,
            IIndex<JobType, IMessageFactory> jobContextMessageFactories,
            ILogger logger,
            ICrossLoadingService crossLoadingService,
            IDateTimeProvider dateTimeProvider,
            IExternalDataScheduleService externalDataScheduleService)
        {
            _jobSchedulerStatusManager = jobSchedulerStatusManager;
            _jobQueueManager = jobQueueManager;
            _messagingService = messagingService;
            _auditor = auditor;
            _jobContextMessageFactories = jobContextMessageFactories;
            _logger = logger;
            _crossLoadingService = crossLoadingService;
            _dateTimeProvider = dateTimeProvider;
            _externalDataScheduleService = externalDataScheduleService;
        }

        public async Task ProcessNextJobAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            while (true)
            {
                try
                {
                    if (await _jobSchedulerStatusManager.IsJobQueueProcessingEnabledAsync())
                    {
                        await QueueAnyNewReferenceDataJobsAsync(cancellationToken);

                        await ProcessAnyNewJobsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured in job scheduler - will continue to pick new jobs", ex);
                }

                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(1000);
            }
        }

        public async Task MoveJobForProcessingAsync(Job job)
        {
            if (job == null)
            {
                return;
            }

            _logger.LogInfo($"Job id: {job.JobId} received for moving to queue", jobIdOverride: job.JobId);

            var message = await _jobContextMessageFactories[job.JobType].CreateMessageParametersAsync(job.JobId);

            try
            {
                var jobStatusUpdated = await _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing);

                _logger.LogInfo($"Job id: {job.JobId} status updated successfully", jobIdOverride: job.JobId);

                if (jobStatusUpdated)
                {
                    try
                    {
                        await _messagingService.SendMessageAsync(message);
                        await _auditor.AuditAsync(message.JobContextMessage, AuditEventType.JobSubmitted);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Job id: {job.JobId} sending to service bus failed", ex, jobIdOverride: job.JobId);
                        await _auditor.AuditAsync(
                            message.JobContextMessage,
                            AuditEventType.ServiceFailed,
                            $"Failed to send message to Service bus queue with exception : {ex}");

                        await _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.Failed);
                    }
                }
                else
                {
                    _logger.LogWarning($"Job id : {job.JobId} failed to send to service bus", jobIdOverride: job.JobId);
                    await _auditor.AuditAsync(
                        message.JobContextMessage,
                        AuditEventType.JobFailed,
                        "Failed to update job status, no message is added to the service bus queue");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Job id: {job.JobId}", exception, jobIdOverride: job.JobId);
                await _auditor.AuditAsync(
                    message.JobContextMessage,
                    AuditEventType.ServiceFailed,
                    $"Failed to update job status with exception : {exception}");
            }
        }

        private async Task QueueAnyNewReferenceDataJobsAsync(CancellationToken cancellationToken)
        {
            IEnumerable<JobType> jobTypes = await _externalDataScheduleService.GetJobs(true, cancellationToken);
            foreach (JobType jobType in jobTypes)
            {
                Job refDataJob = new Job
                {
                    DateTimeSubmittedUtc = _dateTimeProvider.GetNowUtc(),
                    JobType = jobType,
                    Priority = 1,
                    Status = JobStatusType.Ready,
                    SubmittedBy = "System"
                };

                /* long id = */await _jobQueueManager.AddJob(refDataJob);
            }
        }

        private async Task ProcessAnyNewJobsAsync()
        {
            IEnumerable<Job> jobs = await _jobQueueManager.GetJobsByPriorityAsync(25);

            foreach (Job job in jobs)
            {
                _logger.LogInfo($"Got job id: {job.JobId}", jobIdOverride: job.JobId);
                await MoveJobForProcessingAsync(job);
                await MoveJobForCrossLoadingAsync(job);
            }
        }

        private async Task MoveJobForCrossLoadingAsync(Job job)
        {
            if (job.CrossLoadingStatus.HasValue)
            {
                var result = await _crossLoadingService.SendMessageForCrossLoadingAsync(job.JobId);
                if (result)
                {
                    _logger.LogInfo($"Sent job id: {job.JobId} for cross loading", jobIdOverride: job.JobId);
                    await _jobQueueManager.UpdateCrossLoadingStatus(job.JobId, JobStatusType.MovedForProcessing);
                }
            }
        }
    }
}