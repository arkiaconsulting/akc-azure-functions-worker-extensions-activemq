using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using System;

namespace Akc.Azure.Functions.Worker.Extensions.ActiveMQ
{
    /// <summary>
    /// ActiveMQ trigger binding attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ActiveMQTriggerAttribute : TriggerBindingAttribute
    {
        /// <summary>
        /// Endpoint in the form 'amqp://localhost:5672' (%...% placeholder supported)
        /// </summary>
        public string Connection { get; }

        /// <summary>
        /// The name of the queue to listen to (%...% placeholder supported)
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// The username to use for authentication (%...% placeholder supported)
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// The password to use for authentication (%...% placeholder supported)
        /// </summary>
        public string Password { get; }

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
