using ActiveMQBinding.Config;
using ActiveMQBinding.Context;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ActiveMQBinding.Binding
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
            var activeMQTriggerAttribute = TypeUtility.GetResolvedAttribute<ActiveMQTriggerAttribute>(context.Parameter);

            if (activeMQTriggerAttribute is null)
            {
                return null;
            }

            return new ActiveMQListenerTriggerBinding(
                new ActiveMQTriggerContext(
                    await _activeMQExtensionConfigProvider.CreateConnection(activeMQTriggerAttribute),
                    activeMQTriggerAttribute),
                context.Parameter.ParameterType,
                _logger);
        }
    }
}
