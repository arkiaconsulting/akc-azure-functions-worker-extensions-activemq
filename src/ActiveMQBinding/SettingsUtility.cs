using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using System;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    internal static class SettingsUtility
    {
        public static string ResolveString(INameResolver nameResolver, string setting, string settingName)
        {
            if (nameResolver is null)
            {
                throw new ArgumentNullException(nameof(nameResolver));
            }

            if (string.IsNullOrWhiteSpace(setting))
            {
                throw new ArgumentNullException(settingName);
            }

            if (nameResolver.TryResolveWholeString(setting, out var resolvedString))
            {
                return resolvedString;
            }

            return setting;
        }

        public static string ResolveConnectionStringOrSetting(IConfiguration configuration, string settingName)
        {
            var value = configuration.GetConnectionStringOrSetting(settingName);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"The setting '{settingName}' is missing or empty");
            }

            return value;
        }
    }
}
