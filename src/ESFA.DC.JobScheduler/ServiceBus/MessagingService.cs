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
        private readonly IQueuePublishService<JobContextMessage> _queuePublishService;
        private readonly IReadOnlyPolicyRegistry<string> _pollyRegistry;

        public MessagingService(IQueuePublishService<JobContextMessage> queuePublishService, IReadOnlyPolicyRegistry<string> pollyRegistry)
        {
            _queuePublishService = queuePublishService;
            _pollyRegistry = pollyRegistry;
        }

        public async Task SendMessagesAsync(JobContextMessage message)
        {
            var policy = _pollyRegistry.Get<IAsyncPolicy>("ServiceBusRetryPolicy");
            await policy.ExecuteAsync(async () =>
                {
                    await _queuePublishService.PublishAsync(message);
                });
        }
    }
}