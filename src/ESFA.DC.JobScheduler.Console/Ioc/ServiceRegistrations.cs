using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ESFA.DC.Auditing;
using ESFA.DC.Auditing.Dto;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.Console.Settings;
using ESFA.DC.JobScheduler.QueueHandler;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;

namespace ESFA.DC.JobScheduler.Console.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessagingService>().As<IMessagingService>().InstancePerLifetimeScope();
            builder.RegisterType<JobQueueManager.JobQueueManager>().As<IJobQueueManager>().InstancePerLifetimeScope();
            builder.RegisterType<QueueHandler.QueueHandler>().As<IQueueHandler>().InstancePerLifetimeScope();
            builder.RegisterType<Auditor>().As<IAuditor>().InstancePerLifetimeScope();
            builder.RegisterType<JobSchedulerStatusManager>().As<IJobSchedulerStatusManager>().InstancePerLifetimeScope();
            builder.RegisterType<JsonSerializationService>().As<ISerializationService>().InstancePerLifetimeScope();

            //builder.RegisterType<JobSchedularStatusManager>().As<IJobSchedularStatusManager>();
            //builder.RegisterType<KeyValueRepository>().As<IKeyValueRepository>().InstancePerLifetimeScope();
            //builder.RegisterType<KeyValueStorageConfig>().As<ISqlServerKeyValuePersistenceServiceConfig>().InstancePerLifetimeScope();
            //builder.RegisterType<SqlServerKeyValuePersistenceService>().As<IKeyValuePersistenceService>().InstancePerLifetimeScope();

            builder.RegisterType<QueuePublishService<JobContextMessage>>().As<IQueuePublishService<JobContextMessage>>().SingleInstance();
            builder.RegisterType<QueuePublishService<AuditingDto>>().As<IQueuePublishService<AuditingDto>>().SingleInstance();

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
