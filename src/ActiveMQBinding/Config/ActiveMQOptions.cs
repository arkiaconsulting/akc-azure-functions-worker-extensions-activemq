namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Config
{
    /// <summary>
    /// Options for configuring the ActiveMQ binding.
    /// </summary>
    public sealed class ActiveMQOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of reconnect attempts. (transport.startupMaxReconnectAttempts)
        /// </summary>
        public int TransportStartupMaxReconnectAttempts { get; set; } = 1;

        /// <summary>
        /// Gets or sets the transport timeout. (transport.timeout)
        /// </summary>
        public int TransportTimeout { get; set; } = 2000;
    }
}
