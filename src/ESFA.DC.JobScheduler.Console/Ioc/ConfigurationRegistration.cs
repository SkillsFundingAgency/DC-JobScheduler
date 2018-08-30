using Autofac;
using ESFA.DC.Job.WebApi.Settings;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.Console.Extensions;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.Queueing.Interface.Configuration;
using Microsoft.Extensions.Configuration;

namespace ESFA.DC.JobScheduler.Console.Ioc
{
    public static class ConfigurationRegistration
    {
        public static void SetupConfigurations(this ContainerBuilder builder, IConfiguration configuration)
        {
            builder.Register(c => configuration.GetConfigSection<JobQueueManagerSettings>())
                .As<JobQueueManagerSettings>().SingleInstance();

            builder.Register(c => configuration.GetConfigSection<IlrQueueConfiguration>())
                .As<IQueueConfiguration>().SingleInstance();

            builder.Register(c => configuration.GetConfigSection<AuditQueueConfiguration>())
                .As<AuditQueueConfiguration>().SingleInstance();

            builder.Register(c => configuration.GetConfigSection<IlrFirstStageMessageTopics>())
                .As<IlrFirstStageMessageTopics>().SingleInstance();

            builder.Register(c => configuration.GetConfigSection<IlrSecondStageMessageTopics>())
                .As<IlrSecondStageMessageTopics>().SingleInstance();

            builder.Register(c => configuration.GetConfigSection<ConnectionStrings>())
                .As<ConnectionStrings>().SingleInstance();
        }
    }
}