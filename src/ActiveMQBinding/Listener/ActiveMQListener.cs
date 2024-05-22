using Akc.Azure.WebJobs.Extensions.ActiveMQ.Services;
using Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers;
using Apache.NMS;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Listener
{
    internal sealed class ActiveMQListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _triggeredFunctionExecutor;
        private readonly Task<IConnection> _connectionTask;
        private readonly string _queueName;
        private readonly ActiveMQConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private CancellationTokenSource _cts;
        private SingleItemFunctionExecutor _functionExecutor;

        public ActiveMQListener(
            ITriggeredFunctionExecutor triggeredFunctionExecutor,
            Task<IConnection> connectionTask,
            string queueName,
            ActiveMQConnectionFactory connectionFactory,
            ILogger logger)
        {
            _triggeredFunctionExecutor = triggeredFunctionExecutor;
            _connectionTask = connectionTask;
            _queueName = queueName;
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting listener");

            var thread = new Thread(RunActiveMQListener)
            {
                IsBackground = true
            };

            var connection = (_connectionTask.Status == TaskStatus.RanToCompletion) ? _connectionTask.Result : (await _connectionTask);
            connection.ExceptionListener += (ex) =>
            {
                _logger.LogError(ex, "ActiveMQ Connection Exception");
            };
            connection.ConnectionResumedListener += async () =>
            {
                _logger.LogInformation("ActiveMQ Connection Resumed, re-starting listening loop");

                _cts = new CancellationTokenSource();
                thread = new Thread(RunActiveMQListener)
                {
                    IsBackground = true
                };

                var (sess, cons) = await _connectionFactory.CreateConsumer(connection, _queueName);

                _functionExecutor = new SingleItemFunctionExecutor(_triggeredFunctionExecutor, _logger, sess);

                _cts = new CancellationTokenSource();

                thread.Start(new ListeningLoopData(cons, _cts.Token));
            };
            connection.ConnectionInterruptedListener += () =>
            {
                _logger.LogWarning("ActiveMQ Connection Interrupted, cancelling listening loop {HashCode}", this.GetHashCode());

                _cts.Cancel();
                _cts.Dispose();

                _functionExecutor.Close(TimeSpan.FromMilliseconds(10 * 1000)).GetAwaiter().GetResult();
                _functionExecutor.Dispose();
            };

            if (string.IsNullOrWhiteSpace(_queueName))
            {
                throw new InvalidOperationException("The name of the queue could not be found");
            }

            var (session, consumer) = await _connectionFactory.CreateConsumer(connection, _queueName);

            _functionExecutor = new SingleItemFunctionExecutor(_triggeredFunctionExecutor, _logger, session);

            _cts = new CancellationTokenSource();
            thread.Start(new ListeningLoopData(consumer, _cts.Token));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping ActiveMQ listener");

            Cancel();

            await _functionExecutor.Close(TimeSpan.FromMilliseconds(10 * 1000));
        }

        public void Cancel() =>
            _cts?.Cancel();

        public void Dispose()
        {
            _logger.LogDebug("Disposing ActiveMQ listener");

            Cancel();
            _cts?.Dispose();
            _functionExecutor?.Dispose();

            var connection = (_connectionTask.Status == TaskStatus.RanToCompletion)
                ? _connectionTask.Result
                : _connectionTask.GetAwaiter().GetResult();
            connection.Close();
            connection.Dispose();
        }

        private void RunActiveMQListener(object parameter)
        {
            var listeningData = (ListeningLoopData)parameter;

            _logger.LogInformation("Start consuming queue");

            while (!listeningData.CancellationToken.IsCancellationRequested)
            {
                var message = listeningData.Consumer.Receive(TimeSpan.FromMilliseconds(500));
                if (message == null)
                {
                    continue;
                }

                _functionExecutor.Add(message);

                _functionExecutor.Flush();
            }

            _logger.LogWarning("Listener loop has ended");
        }

        private sealed class ListeningLoopData
        {
            public IMessageConsumer Consumer { get; }
            public CancellationToken CancellationToken { get; }

            public ListeningLoopData(IMessageConsumer consumer, CancellationToken cancellationToken)
            {
                Consumer = consumer;
                CancellationToken = cancellationToken;
            }
        }
    }
}
