using Akc.Azure.WebJobs.Extensions.ActiveMQ;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(ActiveMQListenerStartup))]

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    internal class ActiveMQListenerStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<ActiveMQExtensionConfigProvider>();
            builder.Services.AddSingleton<ActiveMQConnectionFactory>();
        }
    }
}