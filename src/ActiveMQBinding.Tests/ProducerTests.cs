using Apache.NMS.AMQP;
using FluentAssertions;

namespace ActiveMQBinding.Tests;

public sealed class ProducerTests
{
    [Fact]
    public async Task Test01()
    {
        var connectionFactory = new NmsConnectionFactory("amqp://localhost:5672/");
        using var connection = (NmsConnection)await connectionFactory.CreateConnectionAsync("artemis", "artemis");

        using var session = await connection.CreateSessionAsync();
        var queue = new NmsQueue("Processing.Status");
        using var producer = await session.CreateProducerAsync(queue);

        for (int i = 0; i < 1000; i++)
        {
            var content = /*lang=json,strict*/ $"{{\"id\":\"{i}\"}}";

            var message = await producer.CreateTextMessageAsync(content);

            await producer.SendAsync(message);
        }

        true.Should().BeTrue();
    }
}
