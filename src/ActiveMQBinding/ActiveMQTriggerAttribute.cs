using Microsoft.Azure.WebJobs.Description;
using System;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    /// <summary>
    /// ActiveMQ trigger binding attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class ActiveMQTriggerAttribute : Attribute
    {
        /// <summary>
        /// App setting name that contains the ActiveMQ endpoint
        /// </summary>
        [AutoResolve]
        public string Connection { get; }

        /// <summary>
        /// App setting name that contains the ActiveMQ queue name
        /// </summary>
        [AutoResolve]
        public string QueueName { get; }

        /// <summary>
        /// App setting name that contains the ActiveMQ username
        /// </summary>
        [AutoResolve]
        public string UserName { get; }

        /// <summary>
        /// App setting name that contains the ActiveMQ password
        /// </summary>
        [AutoResolve]
        public string Password { get; }

        internal string ResolvedQueueName { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ActiveMQTriggerAttribute"/>.
        /// </summary>
        /// <param name="connection">Endpoint in the form 'amqp://localhost:5672' (%...% placeholder supported)</param>
        /// <param name="queueName">The name of the queue to listen to (%...% placeholder supported)</param>
        /// <param name="userName">The username to use for authentication (%...% placeholder supported)</param>
        /// <param name="password">The password to use for authentication (%...% placeholder supported)</param>
        public ActiveMQTriggerAttribute(string connection, string queueName, string userName, string password)
        {
            Connection = connection;
            QueueName = queueName;
            UserName = userName;
            Password = password;
        }
    }
}
