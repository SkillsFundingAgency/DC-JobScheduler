using System;
using System.Collections.Generic;
using System.Transactions;
using Autofac;
using ESFA.DC.Auditing;
using ESFA.DC.Auditing.Dto;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobNotifications;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobScheduler.JobContextMessage;
using ESFA.DC.JobScheduler.QueueHandler;
using ESFA.DC.JobScheduler.ServiceBus;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace ESFA.DC.JobScheduler.Console.Ioc
{
    public class ServiceRegistrations : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobContextMapper>().InstancePerLifetimeScope();
            builder.RegisterType<MessagingService>().As<IMessagingService>().InstancePerLifetimeScope();
            builder.RegisterType<JobManager>().As<IJobManager>().InstancePerLifetimeScope();
            builder.RegisterType<FileUploadJobManager>().As<IFileUploadJobManager>().InstancePerLifetimeScope();
            builder.RegisterType<QueueHandler.QueueHandler>().As<IQueueHandler>().InstancePerLifetimeScope();
            builder.RegisterType<JobSchedulerStatusManager>().As<IJobSchedulerStatusManager>().InstancePerLifetimeScope();

            builder.RegisterType<QueuePublishService<JobContextDto>>().As<IQueuePublishService<JobContextDto>>().SingleInstance();
            builder.RegisterType<JsonSerializationService>().As<ISerializationService>().InstancePerLifetimeScope();
            builder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().SingleInstance();
            builder.RegisterType<KeyGenerator.KeyGenerator>().As<IKeyGenerator>().SingleInstance();
            builder.RegisterType<JobContextMessageFactory>().As<JobContextMessageFactory>().SingleInstance();
            builder.RegisterType<EmailNotifier>().As<IEmailNotifier>().InstancePerLifetimeScope();

            builder.Register(c => new QueuePublishService<AuditingDto>(
                    c.Resolve<AuditQueueConfiguration>(),
                    c.Resolve<ISerializationService>()))
                .As<IQueuePublishService<AuditingDto>>();
            builder.RegisterType<Auditor>().As<IAuditor>().InstancePerLifetimeScope();

            builder.Register(context =>
                {
                    var queueManagerSettings = context.Resolve<JobQueueManagerSettings>();
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseSqlServer(
                        queueManagerSettings.ConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions>()
                .SingleInstance();

            builder.Register(context =>
            {
                var logger = Logging.LoggerManager.CreateDefaultLogger();
                return logger;
            })
                .As<ILogger>()
                .InstancePerLifetimeScope();

            builder.Register(context =>
                {
                    var registry = new PolicyRegistry();
                    registry.Add(
                        "ServiceBusRetryPolicy",
                        Policy
                            .Handle<MessagingEntityNotFoundException>()
                            .Or<ServerBusyException>()
                            .Or<ServiceBusCommunicationException>()
                            .Or<TimeoutException>()
                            .Or<TransactionInDoubtException>()
                            .Or<QuotaExceededException>()
                            .WaitAndRetryAsync(
                                3, // number of retries
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                                (exception, timeSpan, retryCount, executionContext) =>
                                {
                                    // TODO: log the error
                                }));
                    return registry;
                }).As<IReadOnlyPolicyRegistry<string>>()
                .SingleInstance();
        }
    }
}
