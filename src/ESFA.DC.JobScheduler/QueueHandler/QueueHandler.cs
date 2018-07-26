using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.Jobs.Model.Enums;
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
        private readonly IIlrJobQueueManager _jobQueueManager;
        private readonly IAuditor _auditor;
        private readonly JobContextMessageFactory _jobContextMessageFactory;
        private readonly ILogger _logger;

        public QueueHandler(
            IMessagingService messagingService,
            IIlrJobQueueManager jobQueueManager,
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

                        {
                            Console.WriteLine("Job not found");
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

        public async Task MoveIlrJobForProcessing(IlrJob job)
        {
            if (job == null)
            {
                return;
            }

            Console.WriteLine($"Job id : {job.JobId} recieved");

            var message = _jobContextMessageFactory.CreateIlrJobContextMessage(job);

            try
            {
                var jobStatusUpdated = _jobQueueManager.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing);

                Console.WriteLine($"Job id : {job.JobId} status updated");

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
                    Console.WriteLine($"Job id : {job.JobId} failed to send to service bus");
                    await _auditor.AuditAsync(message, AuditEventType.JobFailed, "Failed to update job status, no message is added to the service bus queue");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Job id : {job.JobId}, error: {exception}");
                await _auditor.AuditAsync(message, AuditEventType.ServiceFailed, $"Failed to update job status with exception : {exception}");
            }
        }

        public Task MoveReferenceJobForProcessing(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}