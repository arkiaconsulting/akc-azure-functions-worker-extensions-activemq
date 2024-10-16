﻿using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Apache.NMS;
using Apache.NMS.AMQP;
using Apache.NMS.Policies;
using Apache.NMS.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Services
{
    internal sealed class ActiveMQConnectionFactory : IDisposable
    {
        private readonly ConcurrentDictionary<ConnectionOptions, Task<IConnection>> _connectionCache = new ConcurrentDictionary<ConnectionOptions, Task<IConnection>>();
        private readonly ActiveMQOptions _options;
        private readonly ILogger _logger;

        public ActiveMQConnectionFactory(IOptions<ActiveMQOptions> options, ILogger<ActiveMQConnectionFactory> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public Task<IConnection> GetConnection(ConnectionOptions connectionOptions) =>
            _connectionCache.GetOrAdd(connectionOptions, CreateConnectionAsync);

        private async Task<IConnection> CreateConnectionAsync(ConnectionOptions connectionOptions)
        {
            _logger.LogDebug("[ActiveMQConnectionFactory] Creating connection");

            var policy = new RedeliveryPolicy
            {
                BackOffMultiplier = 2,
                InitialRedeliveryDelay = 10000,
                MaximumRedeliveries = 10,
                UseExponentialBackOff = true,
            };

            var connectionFactory = new NmsConnectionFactory(CreateProviderUri(connectionOptions.Endpoint, _options));
            var connection = (NmsConnection)await connectionFactory.CreateConnectionAsync(connectionOptions.UserName, connectionOptions.Password);
            connection.RedeliveryPolicy = policy;

            await connection.StartAsync();

            _logger.LogDebug("[ActiveMQConnectionFactory] Connection started {ConnectionId}", connection.Id);

            return connection;
        }

        public async Task<(ISession, IMessageConsumer)> CreateConsumer(IConnection connection, string queueName)
        {
            _logger.LogDebug("[ActiveMQConnectionFactory] Creating consumer");

            var session = await connection.CreateSessionAsync(AcknowledgementMode.ClientAcknowledge);
            
            _logger.LogDebug("[ActiveMQConnectionFactory] Session created {SessionId}", ((NmsSession)session).SessionInfo.Id);

            var queue = SessionUtil.GetDestination(session, queueName, DestinationType.Queue);

            var consumer = await session.CreateConsumerAsync(queue);

            _logger.LogDebug("[ActiveMQConnectionFactory] Consumer created {ConsumerId}", ((NmsMessageConsumer)consumer).Info.Id);

            return (session, consumer);
        }

        private static string CreateProviderUri(string endpoint, ActiveMQOptions options)
        {
            var connectionStringBuilder = new StringBuilder($"failover:({endpoint})?");
            var parametersDictionary = new Dictionary<string, string>
            {
                { "transport.UseLogging", "false" },
                { "transport.startupMaxReconnectAttempts", options.TransportStartupMaxReconnectAttempts.ToString() },
                { "transport.timeout", options.TransportTimeout.ToString() },
                { "transport.maxReconnectAttempts", "0" },
                { "failover.maxReconnectAttempts", "-1" },
                { "failover.initialReconnectDelay", "1000" },
                { "failover.reconnectDelay", "5000" },
            };

            var queryParameters = string.Join("&", parametersDictionary.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            connectionStringBuilder.Append(queryParameters);

            return connectionStringBuilder.ToString();
        }

        public void Dispose()
        {
            foreach (var connection in _connectionCache.Select(connection => connection.Value.Result))
            {
                try
                {
                    connection.Stop();
                    connection.Dispose();
                }
                catch (Exception)
                {
                    // empty
                }
                finally
                {
                    _logger.LogDebug("[ActiveMQConnectionFactory] Connection disposed {ConnectionId}", ((NmsConnection)connection).Id);
                }
            }

            GC.SuppressFinalize(this);
        }
    }

    internal sealed class ConnectionOptions : IEquatable<ConnectionOptions>
    {
        public string Endpoint { get; }
        public string UserName { get; }
        public string Password { get; }

        private int? _hashCode;

        public ConnectionOptions(string Endpoint, string userName, string password)
        {
            this.Endpoint = Endpoint;
            UserName = userName;
            Password = password;
        }

        public override bool Equals(object obj) => Equals(obj as ConnectionOptions);

        public bool Equals(ConnectionOptions other)
        {
            if (other is null)
            {
                return false;
            }

            return (Endpoint == other.Endpoint)
                && (UserName == other.UserName)
                && (Password == other.Password);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Bug", "S2328:\"GetHashCode\" should not reference mutable fields", Justification = "<Pending>")]
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                _hashCode = (Endpoint, UserName, Password).GetHashCode();
            }

            return _hashCode.Value;
        }
    }
}