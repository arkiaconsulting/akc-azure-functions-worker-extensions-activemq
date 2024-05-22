using Akc.Azure.WebJobs.Extensions.ActiveMQ;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Shared;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ActiveMQFunction;

public class ActiveMQConsumer
{
    private readonly ILogger _logger;

    public ActiveMQConsumer(ILogger<ActiveMQConsumer> logger) =>
        _logger = logger;

    [FunctionName("ActiveMQConsumer")]
    [return: ActiveMQ("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
    public string HandleMessage(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] MyPoco poco)
    {
        _logger.LogInformation("[ActiveMQConsumer] Message received");
        _logger.LogInformation("ID = {ID}", poco.Id);

        return JsonSerializer.Serialize(poco);
    }

    [FunctionName("ActiveMQConsumer2")]
    public void HandleMessage2(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] MyPoco poco,
        [ActiveMQ("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] out string output)
    {
        _logger.LogInformation("[ActiveMQConsumer2] Message received");
        _logger.LogInformation("ID = {ID}", poco.Id);

        output = JsonSerializer.Serialize(poco);
    }

    [FunctionName("ActiveMQConsumer3")]
    [return: ActiveMQ("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
    public NmsTextMessage HandleMessage3(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] MyPoco poco,
        INmsMessageFactory messageFactory)
    {
        _logger.LogInformation("[ActiveMQConsumer3] Message received");
        _logger.LogInformation("ID = {ID}", poco.Id);

        var json = JsonSerializer.Serialize(poco);
        var message = messageFactory.CreateTextMessage(json);
        message.Properties.SetString("OrderId", Guid.NewGuid().ToString());

        return message;
    }

    [FunctionName("ActiveMQConsumer4")]
    [return: ActiveMQ("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
    public NmsBytesMessage HandleMessage4(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] string value,
        INmsMessageFactory messageFactory)
    {
        _logger.LogInformation("[ActiveMQConsumer] Message received");
        _logger.LogInformation("Content: {Content}", value);

        var message = messageFactory.CreateBytesMessage(Encoding.UTF8.GetBytes(value));
        message.Properties.SetString("OrderId", Guid.NewGuid().ToString());

        return message;
    }

    [FunctionName("ActiveMQConsumer5")]
    [return: ActiveMQ("%ActiveMQ:DocsToProcessQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
    public ActiveMQOutputMessage HandleMessage5(
        [ActiveMQTrigger("%ActiveMQ:ProcessingStatusQueue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] string value)
    {
        _logger.LogInformation("[ActiveMQConsumer] Message received");
        _logger.LogInformation("Content: {Content}", value);

        return new()
        {
            Text = value,
            Properties = new Dictionary<string, object>
            {
                { "OrderId", Guid.NewGuid().ToString() }
            }
        };
    }
}

public sealed record MyPoco(string Id);
