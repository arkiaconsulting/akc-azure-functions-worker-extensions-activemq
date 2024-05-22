using Akc.Azure.Functions.Worker.Extensions.ActiveMQ;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ActiveMQFunction;

public class ActiveMQConsumer
{
    private readonly ILogger _logger;

    public ActiveMQConsumer(ILogger<ActiveMQConsumer> logger) =>
        _logger = logger;

    [Function("ActiveMQConsumer1")]
    [ActiveMQOutput("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
    public string HandleActiveMQMessage1(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] MyPoco message)
    {
        _logger.LogInformation("[ActiveMQConsumer] Message received");

        return JsonSerializer.Serialize(message);
    }

    [Function("ActiveMQConsumer2")]
    [ActiveMQOutput("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
    public ActiveMQOutputMessage HandleActiveMQMessage3(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] MyPoco poco)
    {
        _logger.LogInformation("[ActiveMQConsumer] Message received");

        var json = JsonSerializer.Serialize(poco);

        return new ActiveMQOutputMessage
        {
            Text = json,
            Properties = { { "OrderId", Guid.NewGuid().ToString() } }
        };
    }
}

public sealed record MyPoco(string Id)
{
    public override string ToString() => $"Id: {Id}";
}