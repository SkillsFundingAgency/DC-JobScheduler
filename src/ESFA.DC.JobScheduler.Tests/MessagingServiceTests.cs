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
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;
using Moq;
using Polly;
using Polly.Registry;
using Xunit;

namespace ESFA.DC.JobScheduler.Tests
{
    public class MessagingServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SendMessagesAsync_Test(bool isCrossLoaded)
        {
            var topicPublishMock = new Mock<ITopicPublishService<JobContextDto>>();
            topicPublishMock.Setup(x => x.PublishAsync(new JobContextDto(), new Dictionary<string, object>(), "Test")).Returns(Task.CompletedTask);

            var queuePublishMock = new Mock<IQueuePublishService<JobContextDto>>();
            queuePublishMock.Setup(x => x.PublishAsync(new JobContextDto())).Returns(Task.CompletedTask);

            var message = new MessageParameters(JobType.IlrSubmission)
            {
                JobContextMessage = new JobContextMessage()
                {
                    KeyValuePairs = new Dictionary<string, object>(),
                    Topics = new List<ITopicItem>()
                },
                SubscriptionLabel = "Test",
                TopicParameters = new Dictionary<string, object>(),
                IsCrossLoaded = isCrossLoaded
            };

            var indexedTopicsMock = new Mock<IIndex<JobType, ITopicPublishService<JobContextDto>>>();
            indexedTopicsMock.SetupGet(x => x[JobType.IlrSubmission]).Returns(topicPublishMock.Object);

            var indexedQueueMock = new Mock<IIndex<JobType, IQueuePublishService<JobContextDto>>>();
            indexedQueueMock.SetupGet(x => x[JobType.IlrSubmission]).Returns(queuePublishMock.Object);

            var messagingService = new MessagingService(indexedTopicsMock.Object, indexedQueueMock.Object, new JobContextMapper(), new Mock<ILogger>().Object);
            messagingService.SendMessageAsync(message).ConfigureAwait(true);

            topicPublishMock.Verify(x => x.PublishAsync(It.IsAny<JobContextDto>(), It.IsAny<Dictionary<string, object>>(), "Test"), Times.Once);
            if (isCrossLoaded)
            {
                queuePublishMock.Verify(x => x.PublishAsync(It.IsAny<JobContextDto>()), Times.Once);
            }
            else
            {
                queuePublishMock.Verify(x => x.PublishAsync(It.IsAny<JobContextDto>()), Times.Never);
            }
        }
    }
}
