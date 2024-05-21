using Akc.Azure.Functions.Worker.Extensions.ActiveMQ;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ActiveMQFunction;

public class ActiveMQConsumer
{
    private readonly ILogger _logger;

    public ActiveMQConsumer(ILogger<ActiveMQConsumer> logger) =>
        _logger = logger;

    [Function("ActiveMQConsumer")]
    public void HandleActiveMQMessage(
        [ActiveMQTrigger("%ActiveMQ:Endpoint%", "%ActiveMQ:ProcessingStatusQueue%", "%ActiveMQ:UserName%", "%ActiveMQ:Password%")] MyPoco message)
    {
        _logger.LogInformation("Message received");

        _logger.LogInformation("Received {Message}", message.ToString());
    }
}

public sealed record MyPoco(string Id)
{
    public override string ToString() => $"Id: {Id}";
}