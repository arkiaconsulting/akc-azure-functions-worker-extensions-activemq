using Akc.Azure.WebJobs.Extensions.ActiveMQ;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(ActiveMQListenerStartup))]

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    internal class ActiveMQListenerStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddExtension<ActiveMQExtensionConfigProvider>();
    }
}