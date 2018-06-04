﻿using System;
using Autofac;
using Autofac.Builder;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.Console.Ioc;
using ESFA.DC.JobScheduler.QueueHandler;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Polly.Registry;
using Xunit;

namespace ESFA.DC.JobScheduler.Console.Tests
{
    public class IocTests
    {
        [Fact]
        public void Test_AutofacConnfiguration()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<ServiceRegistrations>();

            using (var container = containerBuilder.Build())
            {
                container.IsRegistered(typeof(IMessagingService)).Should().BeTrue();
                container.IsRegistered(typeof(IJobQueueManager)).Should().BeTrue();
                container.IsRegistered(typeof(IQueueHandler)).Should().BeTrue();
                container.IsRegistered(typeof(IJobSchedulerStatusManager)).Should().BeTrue();
                container.IsRegistered(typeof(IQueuePublishService<JobContextDto>)).Should().BeTrue();
                container.IsRegistered(typeof(ISerializationService)).Should().BeTrue();
                container.IsRegistered(typeof(IAuditor)).Should().BeTrue();
                container.IsRegistered(typeof(DbContextOptions)).Should().BeTrue();
                container.IsRegistered(typeof(ILogger)).Should().BeTrue();
                container.IsRegistered(typeof(IReadOnlyPolicyRegistry<string>)).Should().BeTrue();
            }
        }
    }
}