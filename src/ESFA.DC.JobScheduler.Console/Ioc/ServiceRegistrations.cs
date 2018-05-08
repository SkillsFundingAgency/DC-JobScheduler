using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.Console.Settings;
using ESFA.DC.JobScheduler.QueueHandler;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobScheduler.Console.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessagingService>().As<IMessagingService>().InstancePerDependency();
            builder.RegisterType<IJobQueueManager>().As<IJobQueueManager>();
            builder.RegisterType<QueueHandler.QueueHandler>().As<IQueueHandler>().InstancePerLifetimeScope();

            //builder.RegisterType<JobSchedularStatusManager>().As<IJobSchedularStatusManager>();
            //builder.RegisterType<KeyValueRepository>().As<IKeyValueRepository>().InstancePerLifetimeScope();
            //builder.RegisterType<KeyValueStorageConfig>().As<ISqlServerKeyValuePersistenceServiceConfig>().InstancePerLifetimeScope();
            //builder.RegisterType<SqlServerKeyValuePersistenceService>().As<IKeyValuePersistenceService>().InstancePerLifetimeScope();

            builder.RegisterType<QueueSettings>().As<IQueueConfiguration>().SingleInstance();
            builder.RegisterType<QueuePublishService<JobContextMessage>>().As<IQueuePublishService<JobContextMessage>>().SingleInstance();
            builder.Register(context =>
            {
                var logger = ESFA.DC.Logging.LoggerManager.CreateDefaultLogger();
                return logger;
            })
                .As<ILogger>()
                .InstancePerLifetimeScope();
        }
    }
}
