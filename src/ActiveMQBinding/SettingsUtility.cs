using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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
    }
}
