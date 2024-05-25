# ActiveMQ Extension for Azure Functions

## Introduction
This repository contains the bindings and extension for ActiveMQ in Azure Functions. It's based on the [Apache.NMS](https://www.nuget.org/packages/Apache.NMS.AMQP) library.

These extensions has been tested with Artemis ActiveMQ.

There are two types of bindings in this extension:
- `ActiveMQTrigger`: This binding allows you to listen to a queue
- `ActiveMQ`: This binding allows you to send messages to a queue

## Usage

### `ActiveMQTrigger`

The `ActiveMQTrigger` binding allows you to listen to a queue.

> ***Note:***
> The `ActiveMQTrigger` binding is not supported in the Consumption plan. You can use it in the Premium plan or the Dedicated (App Service) plan.

> ***Note:***
> Scaling out to multiple instances is not yet implemented

#### Configuration
- `queueName`: The name of the queue to listen to. Settings placeholders are supported, eg. `%MyQueue%`
- `Connection`: The raw endpoint (eg. amqp://localhost:5672/) or the name of the setting that contains it (eg. `ActiveMQ:Endpoint`)
- `UserName`: The username to use when connecting to the ActiveMQ server, or the setting that contains it (eg. `ActiveMQ:UserName`)
- `Password`: The password to use when connecting to the ActiveMQ server, or the setting that contains it (eg. `ActiveMQ:Password`)

#### Available parameter types
- `string`
- `Custom`: the content of the message is deserialized to the specified type

#### Example
The following example shows a function that listens to a queue, by using a string input

```csharp
[FunctionName("ActiveMQTrigger")]
public static void Run(
    [ActiveMQTrigger("myqueue", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] string message)
{
    Console.WriteLine(message);
}
```

The following example shows a function that listens to a queue, by using a custom input

```csharp
[FunctionName("ActiveMQTrigger")]
public static void Run(
    [ActiveMQTrigger("myqueue", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] Order order)
{
    Console.WriteLine(message);
}
```

### `ActiveMQ`

The `ActiveMQ` binding allows you to send messages to a queue.

#### Configuration
- `queueName`: The name of the queue to listen to. Settings placeholders are supported, eg. `%MyQueue%`
- `Connection`: The raw endpoint (eg. amqp://localhost:5672/) or the name of the setting that contains it (eg. `ActiveMQ:Endpoint`)
- `UserName`: The username to use when connecting to the ActiveMQ server, or the setting that contains it (eg. `ActiveMQ:UserName`)
- `Password`: The password to use when connecting to the ActiveMQ server, or the setting that contains it (eg. `ActiveMQ:Password`)

#### Available parameter types
- Types available in-process: `string`, `NmsTextMessage`, `NmsBytesMessage` and `ActiveMQOutputMessage`.
- Types available in isolated mode: `string` and `ActiveMQOutputMessage`.

*The `ActiveMQOutputMessage` is a type provided by the extension, and allows the client to set the content of the message and its properties*

#### Example
The following examples shows a function that sends a message to a queue in an Azure Function in-process:

```csharp
[FunctionName("MyFunction")]
public static void Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    [ActiveMQ("myqueue", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] out string text)
{
    text = "Hello, world!";
}
```

```csharp
[FunctionName("MyFunction")]
[return: ActiveMQ("myqueue", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
public static string Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
{
    return "Hello, world!";
}
```

```csharp
[FunctionName("MyFunction")]
[return: ActiveMQ("myqueue", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
public static NmsTextMessage Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    INmsMessageFactory messageFactory)
{
    var message = messageFactory.CreateTextMessage("Hello, world!");
    message.Properties.SetString("OrderId", Guid.NewGuid().ToString());

    return message;
}
```

```csharp
[FunctionName("MyFunction")]
public static void Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    [ActiveMQ("myqueue", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")] out NmsBytesMessage message,
    INmsMessageFactory messageFactory)
{
    var message = messageFactory.CreateBytesMessage("Hello, world!");
    message.Properties.SetString("OrderId", Guid.NewGuid().ToString());
}
```

```csharp
[Function("MyFunction")]
[return: ActiveMQOutput("%ActiveMQ:Queue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
public ActiveMQOutputMessage Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
{
    var json = JsonSerializer.Serialize(new { Text = "Hello, world!" });

    return new ActiveMQOutputMessage
    {
        Text = json,
        Properties = { { "OrderId", Guid.NewGuid().ToString() } }
    };
}
```

The following examples shows a function that sends a message to a queue in an Azure Function isolated:

```csharp
[Function("MyFunction")]
[ActiveMQOutput("%ActiveMQ:Queue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
public string Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
{
    return JsonSerializer.Serialize(new { Text = "Hello, world!" });
}
```

```csharp
[Function("MyFunction")]
[ActiveMQOutput("%ActiveMQ:Queue%", Connection = "ActiveMQ:Endpoint", UserName = "ActiveMQ:UserName", Password = "ActiveMQ:Password")]
public ActiveMQOutputMessage Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
{
    var json = JsonSerializer.Serialize(new { Text = "Hello, world!" });

    return new ActiveMQOutputMessage
    {
        Text = json,
        Properties = { { "OrderId", Guid.NewGuid().ToString() } }
    };
}
```

## Extension Configuration

The extension can be configured using the `host.json` file. Here is an example of the configuration:

```json
{
    "version": "2.0",
    "extensions": {
        "activeMQ": {
            "transportTimeout": 5000,
            "transportStartupMaxReconnectAttempts": 1
        }
    }
}
```

## Contributing
This project welcomes contributions and suggestions. 