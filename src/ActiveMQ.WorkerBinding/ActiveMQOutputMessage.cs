using System.Collections.Generic;

namespace Akc.Azure.Functions.Worker.Extensions.ActiveMQ
{
    /// <summary>
    /// Type to used when returning a message to an ActiveMQ output binding
    /// </summary>
    public sealed class ActiveMQOutputMessage
    {
        /// <summary>
        /// The text message body
        /// </summary>
        public string Text { get; set; } = default;

        /// <summary>
        /// The message properties
        /// </summary>
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
