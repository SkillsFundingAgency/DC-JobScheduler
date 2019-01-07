using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.JobScheduler.Console.Ioc;
using ESFA.DC.JobScheduler.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ESFA.DC.JobScheduler.Console
{
    public static class Program
    {
        public static CancellationToken CancellationToken = CancellationToken.None;

        public static async Task Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            var userSettingsFileName = $"appsettings.{Environment.UserName}.json";
            if (File.Exists($"{Directory.GetCurrentDirectory()}/{userSettingsFileName}"))
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
            containerBuilder.RegisterModule<LoggerRegistrations>();
            var container = containerBuilder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var scheduler = scope.Resolve<IJobQueueHandler>();
                await scheduler.ProcessNextJobAsync(CancellationToken);
            }
        }
    }
}