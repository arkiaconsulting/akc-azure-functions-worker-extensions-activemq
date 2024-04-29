using ActiveMQBinding.Context;
using ActiveMQBinding.Triggers;
using Apache.NMS;
using Apache.NMS.AMQP.Message;
using Apache.NMS.Util;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveMQBinding.Listener
{
    internal sealed class ActiveMQListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _triggeredFunctionExecutor;
        private readonly ActiveMQTriggerContext _triggerContext;
        private readonly ILogger _logger;
        private CancellationTokenSource _cts;
        private SingleItemFunctionExecutor _functionExecutor;

        public ActiveMQListener(
            ITriggeredFunctionExecutor triggeredFunctionExecutor,
            ActiveMQTriggerContext triggerContext,
            ILogger logger)
        {
            _triggeredFunctionExecutor = triggeredFunctionExecutor;
            _triggerContext = triggerContext;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting listener");

            var thread = new Thread(RunActiveMQListener)
            {
                IsBackground = true
            };

            _triggerContext.Connection.ExceptionListener += (ex) =>
            {
                _logger.LogError(ex, "ActiveMQ Connection Exception");
            };
            _triggerContext.Connection.ConnectionResumedListener += () =>
            {
                _logger.LogInformation("ActiveMQ Connection Resumed");

                _logger.LogInformation("Re-starting listening loop");
                _cts = new CancellationTokenSource();
                thread = new Thread(RunActiveMQListener)
                {
                    IsBackground = true
                };
                thread.Start(_cts.Token);
            };
            _triggerContext.Connection.ConnectionInterruptedListener += () =>
            {
                _logger.LogInformation("ActiveMQ Connection Interrupted");
                _logger.LogInformation("Cancelling listening loop");

                _cts.Cancel();
                _cts.Dispose();
            };

            var session = await _triggerContext.Connection.CreateSessionAsync(AcknowledgementMode.Transactional);
            var queueName = _triggerContext.ActiveMQTriggerAttribute.QueueName;
            var queue = (IQueue)SessionUtil.GetDestination(session, queueName, DestinationType.Queue);
            var consumer = await session.CreateConsumerAsync(queue);

            _functionExecutor = new SingleItemFunctionExecutor(_triggeredFunctionExecutor, _logger, session);

            _cts = new CancellationTokenSource();
            thread.Start(new ListeningLoopData(queue, consumer, _cts.Token));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stoping ActiveMQ listener");

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
            _triggerContext.Connection.Close();
            _triggerContext.Connection.Dispose();
        }

        private void RunActiveMQListener(object parameter)
        {
            var listeningData = (ListeningLoopData)parameter;

            _logger.LogDebug("Start consuming queue '{QueueName}'", listeningData.Queue.QueueName);

            while (!listeningData.CancellationToken.IsCancellationRequested)
            {
                var message = (NmsTextMessage)listeningData.Consumer.Receive(TimeSpan.FromMilliseconds(500));

                if (message == null)
                {
                    continue;
                }

                _functionExecutor.Add(message);

                _functionExecutor.Flush();
            }

            _logger.LogDebug("Listener loop has ended");
        }

        private sealed class ListeningLoopData
        {
            public IMessageConsumer Consumer { get; }
            public IQueue Queue { get; }
            public CancellationToken CancellationToken { get; }

            public ListeningLoopData(IQueue queue, IMessageConsumer consumer, CancellationToken cancellationToken)
            {
                Queue = queue;
                Consumer = consumer;
                CancellationToken = cancellationToken;
            }
        }
    }
}
