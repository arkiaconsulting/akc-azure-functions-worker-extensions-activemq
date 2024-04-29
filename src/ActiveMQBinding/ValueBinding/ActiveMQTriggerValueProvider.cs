using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ActiveMQBinding.ValueBinding
{
    internal class ActiveMQTriggerValueProvider : IValueProvider
    {
        public Type Type { get; }

        private readonly NmsTextMessage _incomingMessage;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _options;

        public ActiveMQTriggerValueProvider(NmsTextMessage incomingMessage, Type requestedParameterType, ILogger logger)
        {
            Type = requestedParameterType;
            _logger = logger;
            _incomingMessage = incomingMessage;

            _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public Task<object> GetValueAsync()
        {
            _logger.LogDebug("[ValueProvider.GetValue] RequestedType: {RequestedType} IncomingType: {ObjectType}", Type.Name, _incomingMessage.GetType().Name);

            if (Type != typeof(string))
            {
                _logger.LogDebug("Value => Poco");

                var json = _incomingMessage.Text;
                var deserialized = JsonSerializer.Deserialize(json, Type, _options);

                return Task.FromResult(deserialized);
            }

            _logger.LogDebug("Value => message text");

            return Task.FromResult<object>(_incomingMessage.Text);
        }

        public string ToInvokeString() =>
            JsonSerializer.Serialize(_incomingMessage, _options);
    }
}
