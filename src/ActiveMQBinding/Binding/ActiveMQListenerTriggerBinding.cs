using ActiveMQBinding.Context;
using ActiveMQBinding.Listener;
using ActiveMQBinding.ValueBinding;
using Apache.NMS;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActiveMQBinding.Binding
{
    internal class ActiveMQListenerTriggerBinding : ITriggerBinding
    {
        public Type TriggerValueType { get; } = typeof(IMessage);

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = CreateBindingDataContract();

        private readonly ActiveMQTriggerContext _triggerContext;

        private readonly Type _parameterType;
        private readonly ILogger _logger;

        public ActiveMQListenerTriggerBinding(ActiveMQTriggerContext triggerContext, Type parameterType, ILogger logger)
        {
            _triggerContext = triggerContext;
            _parameterType = parameterType;
            _logger = logger;
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            var textMessage = (NmsTextMessage)value;
            var valueBinder = new ActiveMQTriggerValueProvider(textMessage, _parameterType, _logger);

            return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, CreateBindingData(textMessage)));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            IListener listener = new ActiveMQListener(context.Executor, _triggerContext, _logger);

            return Task.FromResult(listener);
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(NmsTextMessage value)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { nameof(value), value } };

            return bindingData;
        }

        private static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                ["Message"] = typeof(IMessage),
            };

            return contract;
        }

        public ParameterDescriptor ToParameterDescriptor() =>
            new TriggerParameterDescriptor
            {
                Name = "ActiveMQ Listener",
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "ActiveMQ",
                    Description = "ActiveMQ Trigger"
                }
            };
    }
}
