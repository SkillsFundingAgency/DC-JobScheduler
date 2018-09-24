using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Interfaces.Models;
using ESFA.DC.Queueing.Interface;
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
            var topicPublishMock = new Mock<ITopicPublishService<JobContextDto>>();
            topicPublishMock.Setup(x => x.PublishAsync(new JobContextDto(), new Dictionary<string, object>(), "Test")).Returns(Task.CompletedTask);

            var message = new MessageParameters(JobType.IlrSubmission)
            {
                JobContextMessage = new JobContextMessage()
                {
                    KeyValuePairs = new Dictionary<string, object>(),
                    Topics = new List<ITopicItem>()
                },
                SubscriptionLabel = "Test",
                TopicParameters = new Dictionary<string, object>()
            };

            var indexedMock = new Mock<IIndex<JobType, ITopicPublishService<JobContextDto>>>();
            indexedMock.SetupGet(x => x[JobType.IlrSubmission]).Returns(topicPublishMock.Object);

            var messagingService = new MessagingService(indexedMock.Object, new JobContextMapper());
            messagingService.SendMessageAsync(message).ConfigureAwait(true);

            topicPublishMock.Verify(x => x.PublishAsync(It.IsAny<JobContextDto>(), It.IsAny<Dictionary<string, object>>(), "Test"), Times.Once);
        }
    }
}
