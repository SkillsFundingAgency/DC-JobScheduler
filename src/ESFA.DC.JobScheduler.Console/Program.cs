using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ESFA.DC.JobScheduler.Console.Ioc;
using ESFA.DC.JobScheduler.QueueHandler;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog.Enrichers;

namespace ESFA.DC.JobScheduler.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environmentName == null || environmentName.Equals("development", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddJsonFile($"appsettings.{Environment.UserName}.json");
            }
            else
            {
                config.AddJsonFile("appsettings.json");
            }

            var configurationBuilder = config.Build();
            var configurationModule = new Autofac.Configuration.ConfigurationModule(configurationBuilder);

            containerBuilder.RegisterModule(configurationModule);
            containerBuilder.SetupConfigurations(configurationBuilder);
            containerBuilder.RegisterModule<ServiceRegistrations>();
            var container = containerBuilder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var schedular = scope.Resolve<IQueueHandler>();
                schedular.ProcessNextJobAsync();
            }

            System.Console.ReadLine();
        }
    }
}