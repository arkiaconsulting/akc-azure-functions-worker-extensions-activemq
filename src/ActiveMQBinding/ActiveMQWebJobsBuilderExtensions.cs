using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    internal static class ActiveMQWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddActiveMQ(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<ActiveMQExtensionConfigProvider>()
                .ConfigureOptions<ActiveMQOptions>((config, path, options) =>
                    config.GetSection(path).Bind(options));

            builder.Services.AddSingleton<ActiveMQConnectionFactory>();

            return builder;
        }
    }
}
