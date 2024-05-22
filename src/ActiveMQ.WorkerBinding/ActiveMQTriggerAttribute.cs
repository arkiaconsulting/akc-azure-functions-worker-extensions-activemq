using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Akc.Azure.Functions.Worker.Extensions.ActiveMQ
{
    /// <summary>
    /// ActiveMQ trigger binding attribute
    /// </summary>
    public class ActiveMQTriggerAttribute : TriggerBindingAttribute
    {
        /// <summary>
        /// App setting name that contains the ActiveMQ endpoint
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// The name of the queue to listen to (%...% placeholder supported)
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// App setting name that contains the ActiveMQ username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// App setting name that contains the ActiveMQ password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ActiveMQTriggerAttribute"/>.
        /// </summary>
        /// <param name="queueName">The name of the queue to listen to (%...% placeholder supported)</param>
        public ActiveMQTriggerAttribute(string queueName) => QueueName = queueName;
    }
}
