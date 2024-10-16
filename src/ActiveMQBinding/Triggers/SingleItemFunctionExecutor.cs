using Apache.NMS;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers
{
    internal sealed class SingleItemFunctionExecutor : IDisposable
    {
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly ILogger _logger;
        private readonly Channel<IMessage[]> _channel;
        private readonly CancellationTokenSource _cts;
        private readonly SemaphoreSlim _readerFinished = new SemaphoreSlim(0, 1);
        private readonly List<IMessage> _currentBatch = new List<IMessage>();

        public SingleItemFunctionExecutor(ITriggeredFunctionExecutor executor, ILogger logger)
        {
            _executor = executor;
            _logger = logger;
            _cts = new CancellationTokenSource();

            _channel = Channel.CreateBounded<IMessage[]>(new BoundedChannelOptions(1)
            {
                SingleReader = true,
                SingleWriter = true,
            });
            Task.Run(async () =>
            {
                try
                {
                    await ReaderAsync(_channel.Reader, _cts.Token, _logger);
                }
                catch (Exception ex)
                {
                    // Channel reader will throw OperationCanceledException if cancellation token is cancelled during a call
                    if (!(ex is OperationCanceledException))
                    {
                        _logger.LogError(ex, "Function executor error while processing channel");
                    }
                }
                finally
                {
                    _readerFinished.Release();
                }
            });
        }

        public void Add(IMessage message) => _currentBatch.Add(message);

        public void Flush()
        {
            var items = _currentBatch.ToArray();
            _currentBatch.Clear();

            while (!_cts.IsCancellationRequested)
            {
                if (_channel.Writer.TryWrite(items))
                {
                    break;
                }

                Thread.Sleep(50);
            }
        }

        public async Task Close(TimeSpan timeout)
        {
            _logger.LogDebug("Closing function executor");

            try
            {
                _cts.Cancel();
                _channel.Writer.TryComplete();
                await _readerFinished.WaitAsync(timeout);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while closing function executor");
            }
        }

        public void Dispose()
        {
            _logger.LogDebug("Disposing function executor");

            Close(TimeSpan.Zero).GetAwaiter().GetResult();
            _cts.Dispose();
        }

        private async Task ReaderAsync(ChannelReader<IMessage[]> reader, CancellationToken cancellationToken, ILogger logger)
        {
            while (!cancellationToken.IsCancellationRequested && await reader.WaitToReadAsync(cancellationToken))
            {
                while (!cancellationToken.IsCancellationRequested && reader.TryRead(out var itemsToExecute))
                {
                    try
                    {
                        await ProcessItems(itemsToExecute, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in executor reader");
                    }
                }
            }

            logger.LogInformation("Exiting reader {ProcessName}", nameof(SingleItemFunctionExecutor));
        }

        private async Task ProcessItems(IMessage[] messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                var triggerData = new TriggeredFunctionData
                {
                    TriggerValue = message,
                };

                var result = await _executor.TryExecuteAsync(triggerData, cancellationToken);

                if (!result.Succeeded)
                {
                    _logger.LogError(result.Exception, "Function execution failed");
                }

                await message.AcknowledgeAsync();
            }
        }
    }
}
