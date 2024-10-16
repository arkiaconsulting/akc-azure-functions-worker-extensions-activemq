using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Apache.NMS;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Bindings
{
    internal sealed class ActiveMQAsyncCollectorAsyncConverter<T> : IConverter<ActiveMQAttribute, IAsyncCollector<T>>
    {
        private readonly ActiveMQConnectionFactory _connectionFactory;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;

        public ActiveMQAsyncCollectorAsyncConverter(ActiveMQConnectionFactory connectionFactory, INameResolver nameResolver, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _nameResolver = nameResolver;
            _logger = logger;
        }

        public IAsyncCollector<T> Convert(ActiveMQAttribute input)
        {
            var connectionOptions = new ConnectionOptions(input.Connection, input.UserName, input.Password);
            var queueName = SettingsUtility.ResolveString(_nameResolver, input.QueueName, nameof(input.QueueName));

            return new ActiveMQAsyncCollector<T>(_connectionFactory.GetConnection(connectionOptions), queueName, _logger);
        }
    }

    internal class ActiveMQAsyncCollector<T> : IAsyncCollector<T>
    {
        private readonly Task<IConnection> _connectionTask;
        private readonly string _queueName;
        private readonly ILogger _logger;
        private IConnection _connection;

        public ActiveMQAsyncCollector(Task<IConnection> connectionTask, string queueName, ILogger logger)
        {
            _connectionTask = connectionTask;
            _queueName = queueName;
            _logger = logger;
        }

        public async Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("[ActiveMQAsyncCollector] Publishing item to {ActiveMQQueue} queue", _queueName);

            if (_connection is null)
            {
                _connection = (_connectionTask.Status == TaskStatus.RanToCompletion) ? _connectionTask.Result : (await _connectionTask);
            }

            if (!(item is MessageBuilder messageBuilder))
            {
                var ex = new InvalidOperationException($"Expected item to be of type '{nameof(NmsBytesMessage)}'");
                _logger.LogError(ex, "[ActiveMQAsyncCollector] Error occurred while publishing item to {ActiveMQQueue} queue", _queueName);

                throw ex;
            }

            using (var session = await _connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge))
            {
                var destination = await session.GetQueueAsync(_queueName);
                using (var producer = await session.CreateProducerAsync(destination))
                {
                    var message = await messageBuilder.Build(producer);
                    await producer.SendAsync(message);
                }
            }

            _logger.LogTrace("[ActiveMQAsyncCollector] Item successfully published to {ActiveMQQueue}", _queueName);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
