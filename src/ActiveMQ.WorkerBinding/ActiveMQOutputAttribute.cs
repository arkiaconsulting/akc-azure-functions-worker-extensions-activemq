using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Akc.Azure.Functions.Worker.Extensions.ActiveMQ
{
    /// <summary>
    /// ActiveMQ output binding attribute
    /// </summary>
    public sealed class ActiveMQOutputAttribute : OutputBindingAttribute
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
        /// Initializes a new <see cref="ActiveMQOutputAttribute"/>.
        /// </summary>
        /// <param name="queueName">The name of the queue to listen to (%...% placeholder supported)</param>
        public ActiveMQOutputAttribute(string queueName) => QueueName = queueName;
    }
}
