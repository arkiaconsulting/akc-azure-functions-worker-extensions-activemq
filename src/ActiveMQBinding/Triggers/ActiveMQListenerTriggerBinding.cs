using Akc.Azure.WebJobs.Extensions.ActiveMQ.Listener;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.ValueBinding;
using Apache.NMS;
using Apache.NMS.AMQP;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers
{
    internal class ActiveMQListenerTriggerBinding : ITriggerBinding
    {
        public Type TriggerValueType { get; } = typeof(IMessage);

        public IReadOnlyDictionary<string, Type> BindingDataContract => CreateBindingDataContract();

        private readonly ActiveMQTriggerAttribute _attribute;
        private readonly ParameterInfo _parameter;
        private readonly IConverterManager _converterManager;
        private readonly Task<IConnection> _connectionTask;
        private readonly string _queueName;
        private readonly ActiveMQConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public ActiveMQListenerTriggerBinding(
            ActiveMQTriggerAttribute attribute,
            ParameterInfo parameter,
            IConverterManager converterManager,
            Task<IConnection> connectionTask,
            string queueName,
            ActiveMQConnectionFactory connectionFactory,
            ILogger logger)
        {
            _attribute = attribute;
            _parameter = parameter;
            _converterManager = converterManager;
            _connectionTask = connectionTask;
            _queueName = queueName;
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            _logger.LogTrace("[ActiveMQListenerTriggerBinding] Binding parameter {ParameterType} {ParameterName} {ParameterValueType}", _parameter.Name, _parameter.ParameterType.Name, value.GetType().Name);

            var message = (IMessage)value;

            var converter = _converterManager.GetConverter<ActiveMQTriggerAttribute>(value.GetType(), _parameter.ParameterType);
            var convertedValue = await converter(message, _attribute, context);

            var bindingData = await CreateBindingData();

            return new TriggerData(new ActiveMQTriggerValueProvider(_parameter, message, convertedValue, _logger), bindingData);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            IListener listener = new ActiveMQListener(context.Executor, _connectionTask, _queueName, _connectionFactory, _logger);

            return Task.FromResult(listener);
        }

        private IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            _logger.LogTrace("[ActiveMQListenerTriggerBinding] Creating binding data contract");

            var contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "MessageFactory", typeof(INmsMessageFactory) }
            };

            return contract;
        }

        private async Task<IReadOnlyDictionary<string, object>> CreateBindingData()
        {
            _logger.LogTrace("[ActiveMQListenerTriggerBinding] Creating binding data");

            var connection = (_connectionTask.Status == TaskStatus.RanToCompletion) ? _connectionTask.Result : await _connectionTask;

            var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "MessageFactory", ((NmsConnection)connection).MessageFactory }
            };

            return data;
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
