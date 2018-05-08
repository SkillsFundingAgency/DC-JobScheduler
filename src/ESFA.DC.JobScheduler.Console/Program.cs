using Autofac;
using Autofac.Extensions.DependencyInjection;
using ESFA.DC.JobScheduler.Console.Ioc;
using ESFA.DC.JobScheduler.QueueHandler;
using Microsoft.Extensions.DependencyInjection;

namespace ESFA.DC.JobScheduler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule<ServiceRegistrations>();

            var services = new ServiceCollection();
            containerBuilder.Populate(services);
            var container = containerBuilder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var schedular = scope.Resolve<IQueueHandler>();
                schedular.ProcessNextJobAsync();
            }
        }
    }
}