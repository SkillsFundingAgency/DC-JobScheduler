using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
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
            var queuePublishServiceMock = new Mock<IQueuePublishService<JobContextDto>>();
            queuePublishServiceMock.Setup(x => x.PublishAsync(new JobContextDto())).Returns(Task.CompletedTask);

            var message = new JobContext.JobContextMessage()
            {
                KeyValuePairs = new ConcurrentDictionary<string, object>(),
                Topics = new List<ITopicItem>(),
                JobId = 1,
            };

            var polyMock = new Mock<IReadOnlyPolicyRegistry<string>>();
            polyMock.Setup(x => x.Get<IAsyncPolicy>(It.IsAny<string>())).Returns(Policy.NoOpAsync);

            var messagingService = new MessagingService(queuePublishServiceMock.Object, polyMock.Object, new JobContextMapper());
            messagingService.SendMessagesAsync(message).ConfigureAwait(true);

            queuePublishServiceMock.Verify(x => x.PublishAsync(It.IsAny<JobContextDto>()), Times.Once);
        }
    }
}
