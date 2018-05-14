﻿using Autofac;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.Console.Extensions;
using ESFA.DC.JobScheduler.Console.Settings;
using ESFA.DC.Queueing.Interface;
using Microsoft.Extensions.Configuration;

namespace ESFA.DC.JobScheduler.Console.Ioc
{
    public static class ConfigurationRegistration
    {
        public static void SetupConfigurations(this ContainerBuilder builder, IConfiguration configuration)
        {
            builder.Register(c => configuration.GetConfigSection<JobQueueManagerSettings>())
                .As<JobQueueManagerSettings>().SingleInstance();

            builder.Register(c => configuration.GetConfigSection<QueueConfiguration>())
                .As<IQueueConfiguration>().SingleInstance();
        }
    }
}