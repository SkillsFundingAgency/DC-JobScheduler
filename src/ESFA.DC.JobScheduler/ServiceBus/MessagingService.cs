using System;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ESFA.DC.JobContext;
using ESFA.DC.Queueing.Interface;
using Microsoft.Azure.ServiceBus;
using Polly;
using Polly.Registry;

namespace ESFA.DC.JobScheduler.ServiceBus
{
    public class MessagingService : IMessagingService
    {
        private readonly IQueuePublishService<JobContextDto> _queuePublishService;
        private readonly IReadOnlyPolicyRegistry<string> _pollyRegistry;
        private readonly JobContextMapper _jobContextMapper;

        public MessagingService(IQueuePublishService<JobContextDto> queuePublishService, IReadOnlyPolicyRegistry<string> pollyRegistry, JobContextMapper jobContextMapper)
        {
            _queuePublishService = queuePublishService;
            _pollyRegistry = pollyRegistry;
            _jobContextMapper = jobContextMapper;
        }

        public async Task SendMessagesAsync(JobContext.JobContextMessage message)
        {
            var policy = _pollyRegistry.Get<IAsyncPolicy>("ServiceBusRetryPolicy");
            await policy.ExecuteAsync(async () =>
                {
                    await _queuePublishService.PublishAsync(_jobContextMapper.MapFrom(message));
                });
        }
    }
}