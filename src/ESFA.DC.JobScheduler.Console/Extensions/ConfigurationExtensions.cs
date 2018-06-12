using Microsoft.Extensions.Configuration;

namespace ESFA.DC.JobScheduler.Console.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetConfigSection<T>(this IConfiguration configuration)
        {
            return configuration.GetSection(typeof(T).Name).Get<T>();
        }

        public static T GetConfigSection<T>(this IConfiguration configuration, string sectionName)
        {
            return configuration.GetSection(sectionName).Get<T>();
        }
    }
}
