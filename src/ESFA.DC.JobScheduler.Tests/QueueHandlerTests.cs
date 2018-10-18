using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobSchduler.CrossLoading;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Interfaces.Models;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Queueing.Interface.Configuration;
using Moq;
using Xunit;

namespace ESFA.DC.JobScheduler.Tests
{
    public class QueueHandlerTests
    {
        [Fact]
        public void ProcessNextJobAsync_Test()
        {
            var queueHandler = GetJobQueueHandler();

            Task.Factory.StartNew(() => queueHandler.ProcessNextJobAsync().ConfigureAwait(true)).Wait(TimeSpan.FromSeconds(2));
            //jobQueueManagerMock.Verify(x => x.GetJobByPriority(), Times.AtLeastOnce);
        }

        [Fact]
        public void MoveJobForProcessing_Test()
        {
            var job = new Jobs.Model.Job()
            {
                JobId = 1,
                JobType = JobType.IlrSubmission
            };

            var jobQueueManagerMock = new Mock<IJobManager>();
            jobQueueManagerMock.Setup(x => x.GetJobByPriority()).Returns(new Jobs.Model.Job());
            jobQueueManagerMock.Setup(x => x.UpdateJobStatus(It.IsAny<long>(), It.IsAny<JobStatusType>())).Returns(true);

            var auditorMock = new Mock<IAuditor>();
            auditorMock.Setup(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.ServiceFailed, It.IsAny<string>())).Returns(Task.CompletedTask);

            var messagingServiceMock = new Mock<IMessagingService>();
            messagingServiceMock.Setup(x => x.SendMessageAsync(new MessageParameters(JobType.IlrSubmission)));

            var queueHandler = GetJobQueueHandler(messagingServiceMock.Object, auditorMock.Object, jobQueueManagerMock.Object);

            var task = queueHandler.MoveJobForProcessing(job).ConfigureAwait(true);
            task.GetAwaiter().GetResult();

            jobQueueManagerMock.Verify(x => x.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing), Times.Once);
            messagingServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<MessageParameters>()), Times.Once);
            auditorMock.Verify(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.JobSubmitted, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void MoveJobForProcessing_StatusUpdateFailed_Test()
        {
            var job = new Jobs.Model.Job()
            {
                JobId = 1,
                JobType = JobType.IlrSubmission
            };

            var jobQueueManagerMock = new Mock<IJobManager>();
            jobQueueManagerMock.Setup(x => x.GetJobByPriority()).Returns(new Jobs.Model.Job());
            jobQueueManagerMock.Setup(x => x.UpdateJobStatus(It.IsAny<long>(), It.IsAny<JobStatusType>())).Returns(false);

            var auditorMock = new Mock<IAuditor>();
            auditorMock.Setup(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.ServiceFailed, It.IsAny<string>())).Returns(Task.CompletedTask);

            var messagingServiceMock = new Mock<IMessagingService>();
            messagingServiceMock.Setup(x => x.SendMessageAsync(new MessageParameters(JobType.IlrSubmission)));

            var queueHandler = GetJobQueueHandler(messagingServiceMock.Object, auditorMock.Object, jobQueueManagerMock.Object);

            var task = queueHandler.MoveJobForProcessing(job).ConfigureAwait(true);
            task.GetAwaiter().GetResult();

            jobQueueManagerMock.Verify(x => x.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing), Times.Once);
            messagingServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<MessageParameters>()), Times.Never);
            auditorMock.Verify(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.JobFailed, It.IsAny<string>()), Times.Once);
        }

        private JobQueueHandler GetJobQueueHandler(IMessagingService messagingService = null, IAuditor auditor = null, IJobManager jobQueueManager = null)
        {
            var jobSchedulerStatusManagerMock = new Mock<IJobSchedulerStatusManager>();
            jobSchedulerStatusManagerMock.Setup(x => x.IsJobQueueProcessingEnabledAsync()).ReturnsAsync(true);

            var jobQueueManagerMock = new Mock<IJobManager>();
            jobQueueManagerMock.Setup(x => x.GetJobByPriority()).Returns(new Jobs.Model.Job());

            var auditorMock = new Mock<IAuditor>();
            auditorMock.Setup(x => x.AuditAsync(It.IsAny<JobContext.JobContextMessage>(), AuditEventType.ServiceFailed, It.IsAny<string>())).Returns(Task.CompletedTask);

            var messageFactoryMock = new Mock<IMessageFactory>();
            messageFactoryMock.Setup(x => x.CreateMessageParameters(It.IsAny<long>()))
                .Returns(new MessageParameters(JobType.IlrSubmission));

            var indexedMock = new Mock<IIndex<JobType, IMessageFactory>>();
            indexedMock.SetupGet(x => x[JobType.IlrSubmission]).Returns(messageFactoryMock.Object);

            var queueHandler = new JobQueueHandler(
                messagingService ?? new Mock<IMessagingService>().Object,
                jobQueueManager ?? jobQueueManagerMock.Object,
                auditor ?? auditorMock.Object,
                jobSchedulerStatusManagerMock.Object,
                indexedMock.Object,
                new Mock<ILogger>().Object,
                It.IsAny<ICrossLoadingService>());

            return queueHandler;
        }
    }
}
