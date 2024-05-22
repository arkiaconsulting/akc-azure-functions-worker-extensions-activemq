using Apache.NMS;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.ValueBinding
{
    internal class ActiveMQTriggerValueProvider : IValueProvider
    {
        public Type Type => _parameter.ParameterType;

        private readonly ParameterInfo _parameter;
        private readonly IMessage _mqMessage;
        private readonly object _convertedValue;
        private readonly ILogger _logger;

        public ActiveMQTriggerValueProvider(ParameterInfo parameter, IMessage mqMessage, object convertedValue, ILogger logger)
        {
            if (convertedValue != null && !parameter.ParameterType.IsInstanceOfType(convertedValue))
            {
                throw new InvalidOperationException("value is not of the correct type.");
            }

            _logger = logger;
            _parameter = parameter;
            _mqMessage = mqMessage;
            _convertedValue = convertedValue;
        }

        public Task<object> GetValueAsync() => Task.FromResult(_convertedValue);

        public string ToInvokeString()
        {
            if (_mqMessage is ITextMessage textMessage)
            {
                return textMessage.Text;
            }
            else if (_mqMessage is IBytesMessage bytesMessage)
            {
                var bytes = new byte[bytesMessage.BodyLength];
                bytesMessage.ReadBytes(bytes);

                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                _logger.LogWarning("Unsupported message type: {MessageType}", _mqMessage.GetType().Name);

                return string.Empty;
            }
        }
    }
}
