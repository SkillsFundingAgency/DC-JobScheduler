using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.JobContextMessage;
using ESFA.DC.JobScheduler.Settings;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.JobScheduler.Tests
{
    public class JobContextMessageFactoryTests
    {
        [Fact]
        public void CreateJobContextMessage_Test_Exception()
        {
            var factory = new JobContextMessageFactory(
                It.IsAny<IlrFirstStageMessageTopics>(),
                It.IsAny<IlrSecondStageMessageTopics>());

            var ilrMock = new Mock<IJob>();
            ilrMock.SetupGet(x => x.JobType).Returns(JobType.PeriodEnd);
            Assert.Throws<NotImplementedException>(() => factory.CreateJobContextMessage(ilrMock.Object));
        }

        [Fact]
        public void CreateJobContextMessage_Test_IlrJob()
        {
            var factory = new JobContextMessageFactory(
                new IlrFirstStageMessageTopics(),
                new IlrSecondStageMessageTopics());

            var ilrJob = new IlrJob();

            var result = factory.CreateJobContextMessage(ilrJob);
            result.Should().BeAssignableTo<JobContext.JobContextMessage>();
        }

        [Fact]
        public void CreateIlrJobContextMessage_Test()
        {
            var factory = new JobContextMessageFactory(
                new IlrFirstStageMessageTopics(),
                new IlrSecondStageMessageTopics());

            var ilrJob = new IlrJob()
            {
                FileSize = 123,
                Ukprn = 999,
                JobId = 100,
                StorageReference = "ref",
                FileName = "filename.xml",
                SubmittedBy = "test user",
                DateTimeSubmittedUtc = new System.DateTime(2018, 10, 10)
            };

            var result = factory.CreateIlrJobContextMessage(ilrJob);
            result.Should().BeAssignableTo<JobContext.JobContextMessage>();
            result.JobId.Should().Be(100);
            result.SubmissionDateTimeUtc.Should().Be(new System.DateTime(2018, 10, 10));
            result.KeyValuePairs[JobContextMessageKey.Container].Should().Be("ref");
            result.KeyValuePairs[JobContextMessageKey.FileSizeInBytes].Should().Be(123);
            result.KeyValuePairs[JobContextMessageKey.Filename].Should().Be("filename.xml");
            result.KeyValuePairs[JobContextMessageKey.UkPrn].Should().Be("999");
            result.KeyValuePairs[JobContextMessageKey.Username].Should().Be("test user");
        }

        [Fact]
        public void CreateIlrTopicsList_Test_FirstStage()
        {
            var firstStageTopics = new IlrFirstStageMessageTopics()
            {
                TopicValidation = "Val",
                TopicDeds_TaskGenerateValidationReport = "val_report"
            };

            var factory = new JobContextMessageFactory(
                firstStageTopics,
                new IlrSecondStageMessageTopics());

            var result = factory.CreateIlrTopicsList(true);
            result.Should().BeAssignableTo<IEnumerable<TopicItem>>();
            result.Count.Should().Be(2);
            result.Any(x => x.SubscriptionName == "Val" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "val_report" && x.Tasks != null).Should().BeTrue();
        }

        [Fact]
        public void CreateIlrTopicsList_Test_SecondStage()
        {
            var secondStageTopics = new IlrSecondStageMessageTopics()
            {
                TopicValidation = "Val",
                TopicDeds_TaskGenerateValidationReport = "val_report",
                TopicDeds = "deds",
                TopicDeds_TaskPersistDataToDeds = "persist",
                TopicFunding = "funding",
                TopicReports = "reports"
            };

            var factory = new JobContextMessageFactory(
                new IlrFirstStageMessageTopics(),
                secondStageTopics);

            var result = factory.CreateIlrTopicsList(false);
            result.Should().BeAssignableTo<IEnumerable<TopicItem>>();
            result.Count.Should().Be(6);
            result.Any(x => x.SubscriptionName == "Val" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "val_report" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "deds" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "persist" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "funding" && x.Tasks != null).Should().BeTrue();
            result.Any(x => x.SubscriptionName == "reports" && x.Tasks != null).Should().BeTrue();
        }
    }
}
