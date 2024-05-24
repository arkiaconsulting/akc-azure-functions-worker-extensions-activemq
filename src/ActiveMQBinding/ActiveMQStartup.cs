using Akc.Azure.WebJobs.Extensions.ActiveMQ;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(ActiveMQStartup))]

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    internal class ActiveMQStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddActiveMQ();
    }
}