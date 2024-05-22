using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers
{
    internal class ActiveMQTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly ActiveMQConnectionFactory _connectionFactory;
        private readonly INameResolver _nameResolver;
        private readonly IConverterManager _converterManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ActiveMQTriggerAttributeBindingProvider(
            ActiveMQConnectionFactory connectionFactory,
            INameResolver nameResolver,
            IConverterManager converterManager,
            IConfiguration configuration,
            ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _nameResolver = nameResolver;
            _converterManager = converterManager;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context is null)
            {
                _logger?.LogError("The provided {Context} is null", nameof(TriggerBindingProviderContext));
                throw new ArgumentNullException(nameof(context));
            }

            var parameter = context.Parameter;

            _logger.LogTrace("[ActiveMQTriggerAttributeBindingProvider] Checking parameter {ParameterName} {ParameterType}", parameter.Name, parameter.ParameterType.Name);

            var attribute = parameter.GetCustomAttribute<ActiveMQTriggerAttribute>(inherit: false);

            if (attribute is null)
            {
                _logger.LogTrace("[ActiveMQTriggerAttributeBindingProvider] No {TriggerAttribute} found", nameof(ActiveMQTriggerAttribute));

                return Task.FromResult<ITriggerBinding>(null);
            }

            var connectionOptions = new ConnectionOptions(
                SettingsUtility.ResolveConnectionStringOrSetting(_configuration, attribute.Connection),
                SettingsUtility.ResolveConnectionStringOrSetting(_configuration, attribute.UserName),
                SettingsUtility.ResolveConnectionStringOrSetting(_configuration, attribute.Password)
            );
            var queueName = SettingsUtility.ResolveString(_nameResolver, attribute.QueueName, nameof(attribute.QueueName));

            var binding = new ActiveMQListenerTriggerBinding(attribute, context.Parameter, _converterManager, _connectionFactory.GetConnection(connectionOptions), queueName, _connectionFactory, _logger);

            _logger.LogTrace("Binding created for parameter with name '{ParameterName}' and type '{ParameterType}'", context.Parameter.Name, context.Parameter.ParameterType.Name);

            return Task.FromResult<ITriggerBinding>(binding);
        }
    }
}
