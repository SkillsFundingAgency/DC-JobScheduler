﻿using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.Auditing;
using ESFA.DC.Auditing.Dto;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.CrossLoad;
using ESFA.DC.CrossLoad.Dto;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobContext;
using ESFA.DC.JobNotifications;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.ExternalData;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Interfaces.ExternalData;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobSchduler.CrossLoading;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Queueing.Interface.Configuration;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using Microsoft.EntityFrameworkCore;

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
            builder.RegisterType<JobQueueHandler>().As<IJobQueueHandler>().InstancePerLifetimeScope();
            builder.RegisterType<JobSchedulerStatusManager>().As<IJobSchedulerStatusManager>().InstancePerLifetimeScope();
            builder.RegisterType<CrossLoadingService>().As<ICrossLoadingService>().InstancePerLifetimeScope();

            builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().InstancePerLifetimeScope();
            builder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().SingleInstance();
            builder.RegisterType<EmailNotifier>().As<IEmailNotifier>().InstancePerLifetimeScope();
            builder.RegisterType<EmailTemplateManager>().As<IEmailTemplateManager>().InstancePerLifetimeScope();
            builder.RegisterType<QueuePublishService<MessageCrossLoadDctToDcftDto>>().As<IQueuePublishService<MessageCrossLoadDctToDcftDto>>().InstancePerLifetimeScope();
            builder.RegisterType<CrossLoadMessageMapper>().InstancePerLifetimeScope();
            builder.RegisterType<JsonSerializationService>().As<ISerializationService>().InstancePerLifetimeScope();

            builder.RegisterType<IlrMessageFactory>().Keyed<IMessageFactory>(JobType.IlrSubmission).WithAttributeFiltering().SingleInstance();
            builder.RegisterType<EsfMessageFactory>().Keyed<IMessageFactory>(JobType.EsfSubmission).WithAttributeFiltering().SingleInstance();
            builder.RegisterType<EasMessageFactory>().Keyed<IMessageFactory>(JobType.EasSubmission).WithAttributeFiltering().SingleInstance();
            builder.RegisterType<PeriodEndMessageFactory>().Keyed<IMessageFactory>(JobType.PeriodEnd).WithAttributeFiltering().SingleInstance();
            builder.RegisterType<ReferenceDataMessageFactory>().Keyed<IMessageFactory>(JobType.ReferenceDataFCS).Keyed<IMessageFactory>(JobType.ReferenceDataEPA).WithAttributeFiltering().SingleInstance();

            builder.RegisterType<ReturnCalendarService>().As<IReturnCalendarService>().InstancePerLifetimeScope();
            builder.RegisterType<ExternalDataScheduleService>().As<IExternalDataScheduleService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
            builder.RegisterType<JobSchedulerStatusManager>().As<IJobSchedulerStatusManager>().InstancePerLifetimeScope();
            builder.RegisterType<JobTopicTaskService>().As<IJobTopicTaskService>().InstancePerLifetimeScope();

            builder.Register(c => new QueuePublishService<AuditingDto>(
                    c.Resolve<AuditQueueConfiguration>(),
                    c.Resolve<IJsonSerializationService>()))
                .As<IQueuePublishService<AuditingDto>>();
            builder.RegisterType<Auditor>().As<IAuditor>().InstancePerLifetimeScope();

            builder.Register(c => new QueuePublishService<AuditingDto>(
                    c.Resolve<AuditQueueConfiguration>(),
                    c.Resolve<IJsonSerializationService>()))
                .As<IQueuePublishService<AuditingDto>>();
            builder.RegisterType<Auditor>().As<IAuditor>().InstancePerLifetimeScope();

            builder.RegisterType<JobQueueDataContext>().As<IJobQueueDataContext>();
            builder.Register(context =>
                {
                    var queueManagerSettings = context.Resolve<JobQueueManagerSettings>();
                    var optionsBuilder = new DbContextOptionsBuilder<JobQueueDataContext>();
                    optionsBuilder.UseSqlServer(
                        queueManagerSettings.ConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<JobQueueDataContext>>()
                .SingleInstance();

            builder.Register(context =>
            {
                var logger = Logging.LoggerManager.CreateDefaultLogger();
                return logger;
            }).As<ILogger>().InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<ITopicConfiguration>(JobType.IlrSubmission);
                return new TopicPublishService<JobContextDto>(config, context.Resolve<IJsonSerializationService>());
            }).Keyed<ITopicPublishService<JobContextDto>>(JobType.IlrSubmission).InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<ITopicConfiguration>(JobType.EsfSubmission);
                return new TopicPublishService<JobContextDto>(config, context.Resolve<IJsonSerializationService>());
            }).Keyed<ITopicPublishService<JobContextDto>>(JobType.EsfSubmission).InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<ITopicConfiguration>(JobType.EasSubmission);
                return new TopicPublishService<JobContextDto>(config, context.Resolve<IJsonSerializationService>());
            }).Keyed<ITopicPublishService<JobContextDto>>(JobType.EasSubmission).InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<ITopicConfiguration>(JobType.PeriodEnd);
                return new TopicPublishService<JobContextDto>(config, context.Resolve<IJsonSerializationService>());
            }).Keyed<ITopicPublishService<JobContextDto>>(JobType.PeriodEnd).InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<ITopicConfiguration>(JobType.ReferenceDataFCS);
                return new TopicPublishService<JobContextDto>(config, context.Resolve<IJsonSerializationService>());
            }).Keyed<ITopicPublishService<JobContextDto>>(JobType.ReferenceDataFCS).InstancePerLifetimeScope();

            builder.Register(context =>
            {
                var config = context.ResolveKeyed<ITopicConfiguration>(JobType.ReferenceDataEPA);
                return new TopicPublishService<JobContextDto>(config, context.Resolve<IJsonSerializationService>());
            }).Keyed<ITopicPublishService<JobContextDto>>(JobType.ReferenceDataEPA).InstancePerLifetimeScope();
        }
    }
}
