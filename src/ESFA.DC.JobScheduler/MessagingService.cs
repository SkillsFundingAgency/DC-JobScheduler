using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Autofac.Features.Indexed;
using ESFA.DC.JobContext;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Interfaces.Models;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;
using Microsoft.Azure.ServiceBus;
using Polly;
using Polly.Registry;

namespace ESFA.DC.JobScheduler
{
    public class MessagingService : IMessagingService
    {
        private readonly IIndex<JobType, ITopicPublishService<JobContextDto>> _topicPublishServices;
        private readonly IIndex<JobType, IQueuePublishService<JobContextDto>> _queuePublishServices;
        private readonly ILogger _logger;
        private readonly JobContextMapper _jobContextMapper;

        public MessagingService(
            IIndex<JobType, ITopicPublishService<JobContextDto>> topicPublishServices,
            IIndex<JobType, IQueuePublishService<JobContextDto>> queuePublishServices,
            JobContextMapper jobContextMapper,
            ILogger logger)
        {
            _topicPublishServices = topicPublishServices;
            _jobContextMapper = jobContextMapper;
            _queuePublishServices = queuePublishServices;
            _logger = logger;
        }

        public async Task SendMessageAsync(MessageParameters messageParameters)
        {
            JobContextMessage jobContextMessage = (JobContextMessage)messageParameters.JobContextMessage;
            await _topicPublishServices[messageParameters.JobType].PublishAsync(
                _jobContextMapper.MapFrom(jobContextMessage),
                messageParameters.TopicParameters,
                messageParameters.SubscriptionLabel);

            if (messageParameters.IsCrossLoaded)
            {
                _logger.LogInfo("Senidng message to cross loading queue for job id : {jobContextMessage.JobId}");
                await SendCrossLoadingMessageAsync(jobContextMessage, messageParameters.JobType);
            }
        }

        public async Task SendCrossLoadingMessageAsync(JobContextMessage jobContextMessage, JobType jobType)
        {
            try
            {
                await _queuePublishServices[jobType].PublishAsync(_jobContextMapper.MapFrom(jobContextMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message to cross loading queue for job id : {jobContextMessage.JobId}", ex);
            }
        }
    }
}