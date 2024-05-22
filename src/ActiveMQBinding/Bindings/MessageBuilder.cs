using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Apache.NMS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Bindings
{
    internal sealed class MessageBuilder
    {
        private readonly string _input;
        private readonly ILogger _logger;
        private readonly IDictionary<string, object> _properties;
        private readonly ITextMessage _textMessage;
        private readonly IBytesMessage _bytesMessage;

        private MessageBuilder(ILogger logger) => _logger = logger;

        public MessageBuilder(string input, ILogger logger, IDictionary<string, object> properties = default)
            : this(logger)
        {
            _logger.LogTrace("[MessageBuilder] Creating MessageBuilder from string");

            _input = input;
            _logger = logger;
            _properties = properties ?? new Dictionary<string, object>();
        }

        public MessageBuilder(ITextMessage textMessage, ILogger logger)
            : this(logger)
        {
            _logger.LogTrace("[MessageBuilder] Creating MessageBuilder from TextMessage");

            _textMessage = textMessage;
        }

        public MessageBuilder(IBytesMessage bytesMessage, ILogger logger)
            : this(logger)
        {
            _logger.LogTrace("[MessageBuilder] Creating MessageBuilder from BytesMessage");

            _bytesMessage = bytesMessage;
        }

        public async Task<IMessage> Build(IMessageProducer producer)
        {
            if (_input != null)
            {
                _logger.LogTrace("[MessageBuilder] Building TextMessage from string input");

                return await HandleStringInput(producer);
            }
            else if (_textMessage != null)
            {
                _logger.LogTrace("[MessageBuilder] Building TextMessage from TextMessage");

                return _textMessage;
            }
            else if (_bytesMessage != null)
            {
                _logger.LogTrace("[MessageBuilder] Building BytesMessage from BytesMessage");

                return _bytesMessage;
            }

            throw new InvalidOperationException("Unable to build output message");
        }

        private async Task<IMessage> HandleStringInput(IMessageProducer producer)
        {
            try
            {
                var outputMessage = JsonSerializer.Deserialize<ActiveMQOutputMessage>(_input, Constants.SerializerOptions);

                if (outputMessage.Text == null)
                {
                    return await ProduceRawTextMessage(producer);
                }

                _logger.LogTrace("[MessageBuilder] Creating TextMessage from ActiveMQOutputMessage");

                var message = await producer.CreateTextMessageAsync(outputMessage.Text);
                outputMessage.Properties.ToList().ForEach(kv => message.Properties[kv.Key] = PropertyValueSanitizer(kv.Value));

                return message;
            }
            catch (JsonException ex)
            {
                _logger.LogTrace(ex, "[MessageBuilder] ActiveMQOutputMessage Deserialization failed. creating TextMessage from input string");
            }

            return await ProduceRawTextMessage(producer);
        }

        private async Task<IMessage> ProduceRawTextMessage(IMessageProducer producer)
        {
            _logger.LogTrace("[MessageBuilder] Creating TextMessage from input string");

            var message = await producer.CreateTextMessageAsync(_input);
            _properties.ToList().ForEach(kv => message.Properties[kv.Key] = kv.Value);

            return message;
        }

        private static object PropertyValueSanitizer(object value)
        {
            if (value is JsonElement el)
            {
                return el.ToString();
            }

            return value;
        }
    }
}
