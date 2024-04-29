using ActiveMQBinding.Binding;
using Apache.NMS;
using Apache.NMS.AMQP;
using Apache.NMS.Policies;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ActiveMQBinding.Config
{
    [Extension("ActiveMQ")]
    internal class ActiveMQExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ActiveMQExtensionConfigProvider(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("ActiveMQ"));
        }

        public void Initialize(ExtensionConfigContext context)
        {
            _logger.LogDebug("Initializing ActiveMQExtensionConfigProvider");

            var triggerRule = context.AddBindingRule<ActiveMQTriggerAttribute>();
            triggerRule.BindToTrigger(new ActiveMQListenerTriggerBindingProvider(this, _logger));
        }

        public async Task<IConnection> CreateConnection(ActiveMQTriggerAttribute triggerAttribute)
        {
            _logger.LogDebug("Creating ActiveMQ connection");

            var policy = new RedeliveryPolicy
            {
                BackOffMultiplier = 2,
                InitialRedeliveryDelay = 10000,
                MaximumRedeliveries = 10,
                UseExponentialBackOff = true
            };

            var connectionUrl = GetValueOrSecretFromConfig(triggerAttribute.Connection);
            var userName = GetValueOrSecretFromConfig(triggerAttribute.UserName);
            var password = GetValueOrSecretFromConfig(triggerAttribute.Password);

            var connectionFactory = new NmsConnectionFactory(CreateProviderUri(connectionUrl));
            var connection = (NmsConnection)await connectionFactory.CreateConnectionAsync(userName, password);
            connection.RedeliveryPolicy = policy;

            try
            {
                _logger.LogDebug("Starting ActiveMQ connection");

                await connection.StartAsync();

                _logger.LogDebug("ActiveMQ connection started");
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Could not start ActiveMQ connection");
                throw;
            }

            return connection;
        }

        private string GetValueOrSecretFromConfig(string value)
        {
            if (value.StartsWith("%") && value.EndsWith("%"))
            {
                return _configuration[value.TrimStart().TrimEnd()];
            }

            return value;
        }

        private static string CreateProviderUri(string connectionUrl) =>
            $"failover:({connectionUrl})?transport.UseLogging=true&transport.startupMaxReconnectAttempts=1&transport.timeout=2000&transport.maxReconnectAttempts=0&failover.maxReconnectAttempts=-1&failover.initialReconnectDelay=1000&failover.reconnectDelay=5000";
    }
}
