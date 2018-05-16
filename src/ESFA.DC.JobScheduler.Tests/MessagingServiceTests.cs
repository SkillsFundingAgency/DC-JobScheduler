using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.Queueing.Interface;
using Microsoft.Azure.ServiceBus;
using Moq;
using Polly;
using Polly.Registry;
using Xunit;

namespace ESFA.DC.JobScheduler.Tests
{
    public class MessagingServiceTests
    {
        [Fact]
        public void SendMessagesAsync_Test()
        {
            var queuePublishServiceMock = new Mock<IQueuePublishService<JobContextMessage>>();
            queuePublishServiceMock.Setup(x => x.PublishAsync(It.IsAny<JobContextMessage>())).Returns(Task.CompletedTask);

            var polyMock = new Mock<IReadOnlyPolicyRegistry<string>>();
            polyMock.Setup(x => x.Get<IAsyncPolicy>(It.IsAny<string>())).Returns(Policy.NoOpAsync);

            var messagingService = new MessagingService(queuePublishServiceMock.Object, polyMock.Object);
            messagingService.SendMessagesAsync(It.IsAny<JobContextMessage>()).ConfigureAwait(true);

            queuePublishServiceMock.Verify(x => x.PublishAsync(It.IsAny<JobContextMessage>()), Times.Once);
        }
    }
}
