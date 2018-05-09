using System;
using System.Threading.Tasks;
using System.Transactions;
using ESFA.DC.JobContext;
using ESFA.DC.Queueing.Interface;
using Microsoft.Azure.ServiceBus;
using Polly;

namespace ESFA.DC.JobScheduler.ServiceBus
{
    public class MessagingService : IMessagingService
    {
        private readonly IQueuePublishService<JobContextMessage> _queuePublishService;

        public MessagingService(IQueuePublishService<JobContextMessage> queuePublishService)
        {
            _queuePublishService = queuePublishService;
        }

        public async Task SendMessagesAsync(JobContextMessage message)
        {
            var retryPolicy = Policy
                .Handle<MessagingEntityNotFoundException>()
                .Or<ServerBusyException>()
                //.Or<ServiceBusCommunicationException>()
                .Or<TimeoutException>()
                .Or<TransactionInDoubtException>()
                .Or<QuotaExceededException>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(5), (exc, span) => LogException(exc));

            await _queuePublishService.PublishAsync(message);

            await retryPolicy.ExecuteAsync(async () =>
                {
                    await _queuePublishService.PublishAsync(message);
                });
        }

        private void LogException(Exception ex)
        {
            //TODO: log error
        }
    }
}