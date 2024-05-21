using Akc.Azure.WebJobs.Extensions.ActiveMQ;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ActiveMQFunction;

public class ActiveMQConsumer
{
    private readonly ILogger _logger;

    public ActiveMQConsumer(ILogger<ActiveMQConsumer> logger) =>
        _logger = logger;

    [FunctionName("ActiveMQConsumer")]
    public void HandleRedisMessage(
        [ActiveMQTrigger("%ActiveMQ:Endpoint%", "%ActiveMQ:ProcessingStatusQueue%", "%ActiveMQ:UserName%", "%ActiveMQ:Password%")] MyPoco poco)
    {
        _logger.LogInformation("Message received");
        _logger.LogInformation("ID = {ID}", poco.Id);
    }
}

public sealed record MyPoco(string Id);