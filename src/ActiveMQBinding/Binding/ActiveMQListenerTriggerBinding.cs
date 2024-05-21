using Akc.Azure.WebJobs.Extensions.ActiveMQ.Context;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Listener;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.ValueBinding;
using Apache.NMS;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Binding
{
    internal class ActiveMQListenerTriggerBinding : ITriggerBinding
    {
        public Type TriggerValueType { get; } = typeof(IMessage);

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = CreateBindingDataContract();

        private readonly ActiveMQTriggerContext _triggerContext;
        private readonly ParameterInfo _parameter;
        private readonly ILogger _logger;

        public ActiveMQListenerTriggerBinding(ActiveMQTriggerContext triggerContext, ParameterInfo parameter, ILogger logger)
        {
            _triggerContext = triggerContext;
            _parameter = parameter;
            _logger = logger;
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            var message = (NmsTextMessage)value;
            var bindingData = CreateBindingData(message);

            return Task.FromResult<ITriggerData>(new TriggerData(new ActiveMQTriggerValueProvider(_parameter, message, _logger), bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            IListener listener = new ActiveMQListener(context.Executor, _triggerContext, _logger);

            return Task.FromResult(listener);
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(NmsTextMessage message)
        {
            var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(message.Text)] = message.Text,
            };

            return bindingData;
        }

        private static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(NmsTextMessage.Text)] = typeof(string),
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
