using ActiveMQBinding.Context;
using Apache.NMS;
using Apache.NMS.AMQP.Util.Synchronization;
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

        public ActiveMQListener(
            ITriggeredFunctionExecutor triggeredFunctionExecutor,
            ActiveMQTriggerContext triggerContext,
            ILogger logger)
        {
            _triggeredFunctionExecutor = triggeredFunctionExecutor;
            _triggerContext = triggerContext;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
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

            _cts = new CancellationTokenSource();
            thread.Start(_cts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stoping listener");

            Cancel();

            return Task.CompletedTask;
        }

        public void Cancel() =>
            _cts?.Cancel();

        public void Dispose()
        {
            _logger.LogDebug("Disposing listener");

            Cancel();
            _cts?.Dispose();
        }

        private void RunActiveMQListener(object parameter)
        {
            var cancellationToken = (CancellationToken)parameter;
            using (var session = _triggerContext.Connection.CreateSession(AcknowledgementMode.Transactional))
            {
                var queueName = _triggerContext.ActiveMQTriggerAttribute.QueueName;
                var destination = SessionUtil.GetDestination(session, queueName, DestinationType.Queue);

                _logger.LogDebug("Start consuming queue '{QueueName}'", queueName);
                using (var consumer = session.CreateConsumer(destination))
                {

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var message = consumer.Receive(TimeSpan.FromMilliseconds(500));

                        if (message == null)
                        {
                            continue;
                        }

                        var executionResult = _triggeredFunctionExecutor.TryExecuteAsync(new TriggeredFunctionData
                        {
                            TriggerValue = message
                        }, cancellationToken).GetAsyncResult();

                        if (!executionResult.Succeeded)
                        {
                            _logger.LogError(executionResult.Exception, "Function failed to execute");

                            session.RollbackAsync().GetAsyncResult();

                            continue;
                        }

                        session.CommitAsync().GetAsyncResult();
                    }
                }
            }

            _logger.LogDebug("Listener loop has ended");
        }
    }
}
