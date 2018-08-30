using System;
using System.IO;
using Autofac;
using ESFA.DC.Job.WebApi.Ioc;
using ESFA.DC.JobScheduler.Console.Ioc;
using ESFA.DC.JobScheduler.QueueHandler;
using ESFA.DC.Queueing.Interface.Configuration;
using Microsoft.Extensions.Configuration;

namespace ESFA.DC.JobScheduler.Console
{
    internal class Program
    {
        private static void Main(string[] args)
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
                var schedular = scope.Resolve<IQueueHandler>();
                schedular.ProcessNextJobAsync();
            }

            System.Console.ReadLine();
        }
    }
}