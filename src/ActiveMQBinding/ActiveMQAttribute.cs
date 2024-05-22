using Microsoft.Azure.WebJobs.Description;
using System;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    /// <summary>
    /// ActiveMQ binding attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class ActiveMQAttribute : Attribute
    {
        /// <summary>
        /// Endpoint in the form 'amqp://localhost:5672' (%...% placeholder supported)
        /// </summary>
        [AppSetting]
        public string Connection { get; set; }

        /// <summary>
        /// The name of the queue to listen to (%...% placeholder supported)
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// The username to use for authentication (%...% placeholder supported)
        /// </summary>
        [AppSetting]
        public string UserName { get; set; }

        /// <summary>
        /// The password to use for authentication (%...% placeholder supported)
        /// </summary>
        [AppSetting]
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ActiveMQAttribute"/>.
        /// </summary>
        /// <param name="queueName">The name of the queue to listen to (%...% placeholder supported)</param>
        public ActiveMQAttribute(string queueName) => QueueName = queueName;
    }
}
