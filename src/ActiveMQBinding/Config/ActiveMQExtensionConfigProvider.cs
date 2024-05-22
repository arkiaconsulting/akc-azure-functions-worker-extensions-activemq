using Akc.Azure.WebJobs.Extensions.ActiveMQ.Bindings;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Config
{
    [Extension("ActiveMQ")]
    internal class ActiveMQExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _configuration;
        private readonly INameResolver _nameResolver;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ActiveMQConnectionFactory _connectionFactory;
        private readonly IConverterManager _converterManager;
        private readonly ILogger _logger;

        public ActiveMQExtensionConfigProvider(IConfiguration configuration, INameResolver nameResolver, ILoggerFactory loggerFactory, ActiveMQConnectionFactory connectionFactory, IConverterManager converterManager)
        {
            _configuration = configuration;
            _nameResolver = nameResolver;
            _loggerFactory = loggerFactory;
            _connectionFactory = connectionFactory;
            _converterManager = converterManager;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("ActiveMQ"));
        }

        public void Initialize(ExtensionConfigContext context)
        {
            _logger.LogDebug("[ActiveMQExtensionConfigProvider] Initializing");

            // Register trigger binding provider
            context.AddBindingRule<ActiveMQTriggerAttribute>()
                .AddOpenConverter<NmsTextMessage, OpenType.Poco>(typeof(TextMessageToPocoConverter<>))
                .AddOpenConverter<NmsBytesMessage, OpenType.Poco>(typeof(BytesMessageToPocoConverter<>))
                .AddConverter(new NmsTextMessageToStringConverter())
                .BindToTrigger(new ActiveMQTriggerAttributeBindingProvider(_connectionFactory, _nameResolver, _converterManager, _configuration, _loggerFactory.CreateLogger("ActiveMQTriggerBinding")));

            // Register binding provider
            var bindingRule = context.AddBindingRule<ActiveMQAttribute>();
            // In Isolated process context, the following converter is not supported, as the the type of the source is 'String'
            bindingRule.AddConverter(new ActiveMQOutputMessageToMessageBuilderConverter(_logger));
            bindingRule.AddConverter<string, MessageBuilder>(input => new MessageBuilder(input, _logger));
            bindingRule.AddConverter<NmsTextMessage, MessageBuilder>(input => new MessageBuilder(input, _logger));
            bindingRule.AddConverter<NmsBytesMessage, MessageBuilder>(input => new MessageBuilder(input, _logger));
            bindingRule.BindToCollector<MessageBuilder>(typeof(ActiveMQAsyncCollectorAsyncConverter<>), _connectionFactory, _nameResolver, _loggerFactory.CreateLogger("ActiveMQOutputBinding"));
        }
    }
}
