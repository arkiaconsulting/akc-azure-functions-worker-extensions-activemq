using ActiveMQBinding;
using ActiveMQBinding.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(ActiveMQListenerStartup))]

namespace ActiveMQBinding
{
    internal class ActiveMQListenerStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddExtension<ActiveMQExtensionConfigProvider>();
    }
}