using ActiveMQBinding;
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
        [ActiveMQTrigger("amqp://localhost:5672/", "Processing.Status", "artemis", "artemis")] MyPoco poco)
    {
        _logger.LogInformation("Message received");
        _logger.LogInformation("ID = {ID}", poco.Id);
    }
}

public sealed record MyPoco(string Id);