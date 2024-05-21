using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.ValueBinding
{
    internal class ActiveMQTriggerValueProvider : IValueProvider
    {
        public Type Type { get; }

        private readonly NmsTextMessage _textMessage;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _options;

        public ActiveMQTriggerValueProvider(ParameterInfo parameter, NmsTextMessage textMessage, ILogger logger)
        {
            _logger = logger;
            _textMessage = textMessage;
            _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            Type = parameter.ParameterType;
        }

        public Task<object> GetValueAsync()
        {
            _logger.LogTrace("[ValueProvider.GetValue] RequestedType: {ParameterType} IncomingType: {IncomingValueType}", Type.Name, nameof(NmsTextMessage));

            if (Type.Equals(typeof(NmsTextMessage)))
            {
                _logger.LogTrace("Returning raw NmsTextMessage");

                return Task.FromResult<object>(_textMessage);
            }

            var inputValue = ToInvokeString();

            if (Type.Equals(typeof(string)))
            {
                _logger.LogTrace("Returning NmsTextMessage.Text as string");

                return Task.FromResult<object>(inputValue);
            }
            else
            {
                _logger.LogTrace("Deserializing NmsTextMessage.Text to type '{Type}'", Type.Name);

                try
                {
                    return Task.FromResult(JsonSerializer.Deserialize(inputValue, Type, _options));
                }
                catch (JsonException e)
                {
                    var errorMessage = $@"Binding parameters to complex objects (such as '{Type.Name}') uses Json.NET serialization. The JSON parser failed: {e.Message}";
                    throw new InvalidOperationException(errorMessage, e);
                }
            }
        }

        public string ToInvokeString() => _textMessage.Text;
    }
}
