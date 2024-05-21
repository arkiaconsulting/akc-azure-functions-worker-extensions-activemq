using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Context;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Binding
{
    internal class ActiveMQListenerTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly ActiveMQExtensionConfigProvider _activeMQExtensionConfigProvider;
        private readonly ILogger _logger;

        public ActiveMQListenerTriggerBindingProvider(
            ActiveMQExtensionConfigProvider activeMQExtensionConfigProvider,
            ILogger logger)
        {
            _activeMQExtensionConfigProvider = activeMQExtensionConfigProvider;
            _logger = logger;
        }

        public async Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context is null)
            {
                _logger?.LogError("The provided {Context} is null", nameof(TriggerBindingProviderContext));
                throw new ArgumentNullException(nameof(context));
            }

            _logger.LogTrace("Try create binding for parameter with name '{ParameterName}' and type '{ParameterType}'", context.Parameter.Name, context.Parameter.ParameterType.Name);

            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<ActiveMQTriggerAttribute>(inherit: false);

            if (attribute is null)
            {
                _logger.LogError("No {AttributeName} found on parameter", nameof(ActiveMQTriggerAttribute));
                return null;
            }

            return new ActiveMQListenerTriggerBinding(
                new ActiveMQTriggerContext(
                    await _activeMQExtensionConfigProvider.CreateConnection(attribute), attribute),
                context.Parameter, _logger);
        }
    }
}
