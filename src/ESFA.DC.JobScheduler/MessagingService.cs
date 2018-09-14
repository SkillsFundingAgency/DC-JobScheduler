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
using ESFA.DC.Queueing.Interface;
using Microsoft.Azure.ServiceBus;
using Polly;
using Polly.Registry;

namespace ESFA.DC.JobScheduler
{
    public class MessagingService : IMessagingService
    {
        private readonly IIndex<JobType, ITopicPublishService<JobContextDto>> _topicPublishServices;
        private readonly IReadOnlyPolicyRegistry<string> _pollyRegistry;
        private readonly JobContextMapper _jobContextMapper;

        public MessagingService(IIndex<JobType, ITopicPublishService<JobContextDto>> topicPublishServices, IReadOnlyPolicyRegistry<string> pollyRegistry, JobContextMapper jobContextMapper)
        {
            _topicPublishServices = topicPublishServices;
            _pollyRegistry = pollyRegistry;
            _jobContextMapper = jobContextMapper;
        }

        public async Task SendMessageAsync(MessageParameters messageParameters)
        {
            JobContextMessage jobContextMessage = (JobContextMessage)messageParameters.JobContextMessage;
            await _topicPublishServices[messageParameters.JobType].PublishAsync(
                _jobContextMapper.MapFrom(jobContextMessage),
                messageParameters.TopicParameters,
                messageParameters.SubscriptionLabel);
        }
    }
}