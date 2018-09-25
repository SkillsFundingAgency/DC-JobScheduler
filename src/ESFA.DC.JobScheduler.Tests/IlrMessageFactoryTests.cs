using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.JobScheduler.Tests
{
    public class IlrMessageFactoryTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateMessageParameters_Success(bool isCrossLoaded)
        {
            var job = new FileUploadJob()
            {
                JobType = JobType.IlrSubmission,
                JobId = 10
            };

            var factory = GetFactory(false, job);

            var result = factory.CreateMessageParameters(It.IsAny<long>());

            result.Should().NotBeNull();
            result.JobType.Should().Be(JobType.IlrSubmission);
            result.JobContextMessage.JobId.Should().Be(10);
            result.JobContextMessage.Topics.Count.Should().Be(4);
            result.SubscriptionLabel.Should().Be("Validation");
            result.TopicParameters.ContainsKey("To").Should().Be(true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddExtraKeys_Test(bool isFirstStage)
        {
           var message = new JobContextMessage();
            var job = new FileUploadJob()
            {
                FileSize = 123,
                Ukprn = 999,
                JobId = 100,
                StorageReference = "ref",
                FileName = "filename.xml",
                SubmittedBy = "test user",
                DateTimeSubmittedUtc = new DateTime(2018, 10, 10),
                IsFirstStage = isFirstStage,
            };

            var factory = GetFactory(isFirstStage, job);

            factory.AddExtraKeys(message, job);

            message.KeyValuePairs[JobContextMessageKey.FileSizeInBytes].Should().Be(123);

            if (isFirstStage)
            {
                message.KeyValuePairs[JobContextMessageKey.PauseWhenFinished].Should().Be("1");
            }
            else
            {
                message.KeyValuePairs.ContainsKey(JobContextMessageKey.PauseWhenFinished).Should().Be(false);
            }

            message.KeyValuePairs.ContainsKey(JobContextMessageKey.InvalidLearnRefNumbers).Should().BeTrue();
            message.KeyValuePairs.ContainsKey(JobContextMessageKey.ValidLearnRefNumbers).Should().BeTrue();
            message.KeyValuePairs.ContainsKey(JobContextMessageKey.ValidationErrors).Should().BeTrue();
            message.KeyValuePairs.ContainsKey(JobContextMessageKey.ValidationErrorLookups).Should().BeTrue();
            message.KeyValuePairs.ContainsKey(JobContextMessageKey.FundingAlbOutput).Should().BeTrue();
            message.KeyValuePairs.ContainsKey(JobContextMessageKey.FundingFm35Output).Should().BeTrue();
            message.KeyValuePairs.ContainsKey(JobContextMessageKey.FundingFm25Output).Should().BeTrue();
        }

        [Fact]
        public void CreateIlrTopicsList_Test_FirstStage()
        {
            var factory = GetFactory();

            var result = factory.CreateTopics(true);
            result.Should().BeAssignableTo<IEnumerable<TopicItem>>();
            result.Count.Should().Be(2);
            result.Any(x => x.SubscriptionName == "Val" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks.Any(y => y.Tasks.Any(z => z.Contains("task_validationreports")))).Should().BeTrue();
        }

        [Fact]
        public void CreateIlrTopicsList_Test_SecondStage()
        {
            var factory = GetFactory(false);

            var result = factory.CreateTopics(false);
            result.Should().BeAssignableTo<IEnumerable<TopicItem>>();
            result.Count.Should().Be(4);
            result.Any(x => x.SubscriptionName == "Val" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "deds" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "deds" && x.Tasks.Any(y => y.Tasks.Any(z => z.Contains("task_persist")))).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "funding" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks.Any(y => y.Tasks.Any(z => z.Contains("val_report")))).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks.Any(y => y.Tasks.Any(z => z.Contains("mo_report")))).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks.Any(y => y.Tasks.Any(z => z.Contains("alb_report")))).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks.Any(y => y.Tasks.Any(z => z.Contains("fs_report")))).Should().BeTrue();
        }

        private IlrMessageFactory GetFactory(bool isFirstStage = true, FileUploadJob job = null)
        {
            var mockIFileUploadJobManager = new Mock<IFileUploadJobManager>();
            mockIFileUploadJobManager.Setup(x => x.GetJobById(It.IsAny<long>())).Returns(
                job ?? new FileUploadJob
                {
                    IsFirstStage = isFirstStage,
                    JobId = 10,
                    Ukprn = 1000
                });

            var firstStageTopics = new IlrFirstStageMessageTopics()
            {
                TopicValidation = "Val",
                TopicReports = "reports",
                TopicReports_TaskGenerateValidationReport = "task_validationreports"
            };

            var secondStageTopics = new IlrSecondStageMessageTopics()
            {
                TopicValidation = "Val",
                TopicFunding = "funding",
                TopicDeds_TaskPersistDataToDeds = "task_persist",
                TopicDeds = "deds",
                TopicReports = "reports",
                TopicReports_TaskGenerateValidationReport = "val_report",
                TopicReports_TaskGenerateMainOccupancyReport = "mo_report",
                TopicReports_TaskGenerateAllbOccupancyReport = "alb_report",
                TopicReports_TaskGenerateFundingSummaryReport = "fs_report"
            };

            var mockTopicConfiguration = new Mock<ITopicConfiguration>();
            mockTopicConfiguration.SetupGet(x => x.SubscriptionName).Returns("Validation");

            var factory = new IlrMessageFactory(
                firstStageTopics,
                secondStageTopics,
                new Mock<IKeyGenerator>().Object,
                new Mock<ILogger>().Object,
                mockIFileUploadJobManager.Object,
                mockTopicConfiguration.Object);

            return factory;
        }
    }
}
