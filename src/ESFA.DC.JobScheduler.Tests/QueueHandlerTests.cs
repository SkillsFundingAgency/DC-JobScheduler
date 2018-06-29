using System;
using System.Threading.Tasks;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.JobScheduler.QueueHandler;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.JobStatus.Interface;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Moq;
using Xunit;

namespace ESFA.DC.JobScheduler.Tests
{
    public class QueueHandlerTests
    {
        [Fact]
        public void ProcessNextJobAsync_Test()
        {
            var jobSchedulerStatusManagerMock = new Mock<IJobSchedulerStatusManager>();
            jobSchedulerStatusManagerMock.Setup(x => x.IsJobQueueProcessingEnabledAsync()).ReturnsAsync(true);

            var jobQueueManagerMock = new Mock<IIlrJobQueueManager>();
            jobQueueManagerMock.Setup(x => x.GetJobByPriority()).Returns(new IlrJob());

            var auditorMock = new Mock<IAuditor>();
            auditorMock.Setup(x => x.AuditAsync(It.IsAny<JobContext.JobContextMessage>(), AuditEventType.ServiceFailed, It.IsAny<string>())).Returns(Task.CompletedTask);

            var queueHandler = new QueueHandler.QueueHandler(
                It.IsAny<IMessagingService>(),
                jobQueueManagerMock.Object,
                auditorMock.Object,
                jobSchedulerStatusManagerMock.Object);

            Task.Factory.StartNew(() => queueHandler.ProcessNextJobAsync().ConfigureAwait(true)).Wait(TimeSpan.FromSeconds(2));
            jobQueueManagerMock.Verify(x => x.GetJobByPriority(), Times.AtLeastOnce);
        }

        [Fact]
        public void MoveJobForProcessing_Test_Null()
        {
            var jobQueueManagerMock = new Mock<IIlrJobQueueManager>();
            var queueHandler = new QueueHandler.QueueHandler(
                It.IsAny<IMessagingService>(),
                jobQueueManagerMock.Object,
                It.IsAny<IAuditor>(),
                It.IsAny<IJobSchedulerStatusManager>());

            var task = queueHandler.MoveIlrJobForProcessing(null).ConfigureAwait(true);
            jobQueueManagerMock.Verify(x => x.UpdateJobStatus(It.IsAny<long>(), JobStatusType.MovedForProcessing), Times.Never);
        }

        [Fact]
        public void MoveJobForProcessing_Test()
        {
            var job = new IlrJob()
            {
                JobId = 1
            };

            var jobQueueManagerMock = new Mock<IIlrJobQueueManager>();
            jobQueueManagerMock.Setup(x => x.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing)).Returns(true);

            var auditorMock = new Mock<IAuditor>();
            auditorMock.Setup(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.JobSubmitted, "Job Started")).Returns(Task.CompletedTask);

            var messagingServiceMock = new Mock<IMessagingService>();
            messagingServiceMock.Setup(x => x.SendMessagesAsync(It.IsAny<JobContextMessage>()))
                .Returns(Task.CompletedTask);

            var queueHandler = new QueueHandler.QueueHandler(
                messagingServiceMock.Object,
                jobQueueManagerMock.Object,
                auditorMock.Object,
                It.IsAny<IJobSchedulerStatusManager>());

            var task = queueHandler.MoveIlrJobForProcessing(job).ConfigureAwait(true);
            task.GetAwaiter().GetResult();

            jobQueueManagerMock.Verify(x => x.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing), Times.Once);
            messagingServiceMock.Verify(x => x.SendMessagesAsync(It.IsAny<JobContextMessage>()), Times.Once);
            auditorMock.Verify(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.JobSubmitted, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void MoveJobForProcessing_Test_UpdateJobFailed()
        {
            var job = new IlrJob()
            {
                JobId = 1
            };
            var jobQueueManagerMock = new Mock<IIlrJobQueueManager>();
            jobQueueManagerMock.Setup(x => x.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing)).Returns(false);

            var auditorMock = new Mock<IAuditor>();
            auditorMock.Setup(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.JobFailed, It.IsAny<string>())).Returns(Task.CompletedTask);

            var messagingServiceMock = new Mock<IMessagingService>();
            messagingServiceMock.Setup(x => x.SendMessagesAsync(It.IsAny<JobContextMessage>()))
                .Returns(Task.CompletedTask);

            var queueHandler = new QueueHandler.QueueHandler(
                messagingServiceMock.Object,
                jobQueueManagerMock.Object,
                auditorMock.Object,
                It.IsAny<IJobSchedulerStatusManager>());

            var task = queueHandler.MoveIlrJobForProcessing(job).ConfigureAwait(true);
            task.GetAwaiter().GetResult();

            jobQueueManagerMock.Verify(x => x.UpdateJobStatus(job.JobId, JobStatusType.MovedForProcessing), Times.Once);
            auditorMock.Verify(x => x.AuditAsync(It.IsAny<JobContextMessage>(), AuditEventType.JobFailed, It.IsAny<string>()), Times.Once);
            messagingServiceMock.Verify(x => x.SendMessagesAsync(It.IsAny<JobContextMessage>()), Times.Never);
        }
    }
}
