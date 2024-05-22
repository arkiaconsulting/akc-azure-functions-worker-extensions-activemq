using Akc.Azure.WebJobs.Extensions.ActiveMQ.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers
{
    internal sealed class ActiveMQOutputMessageToMessageBuilderConverter : IConverter<ActiveMQOutputMessage, MessageBuilder>
    {
        private readonly ILogger _logger;

        public ActiveMQOutputMessageToMessageBuilderConverter(ILogger logger) => _logger = logger;
        public MessageBuilder Convert(ActiveMQOutputMessage input)
        {
            _logger.LogTrace("[ActiveMQOutputMessageToMessageBuilderConverter] Converting ActiveMQOutputMessage to MessageBuilder");

            return new MessageBuilder(input.Text, _logger, input.Properties);
        }
    }
}
