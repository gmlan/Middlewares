using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client.Events;

namespace Com.Gmlan.Middlewares.MessageQueue.Client
{
    public abstract class RabbitMqClient
    {
        private readonly string _connectionString;

        protected IDictionary<string, object> Arguments = new Dictionary<string, object>();


        protected RabbitMqClient(string connectionString)
        {
            _connectionString = connectionString;
            Arguments["x-max-priority"] = 10;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;


        public byte[] Encode(string message)
        {
            return Encoding.GetBytes(message);
        }

        public string Decode(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }

        //“host=myServer;port=5672;virtualHost=myVirtualHost;username=mike;password=topsecret”

        public IConnection GetConnection()
        {
            var factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ContinuationTimeout = new TimeSpan(5000),
                NetworkRecoveryInterval = new TimeSpan(10000),
                RequestedHeartbeat = 60
            };

            if (_connectionString.ToLower().StartsWith("amqp://"))
            {
                factory.Uri = new Uri(_connectionString);
                return factory.CreateConnection();
            }

            var parameters = ParseConnnectionParameter(_connectionString);
            factory.UserName = parameters["username"];
            factory.Password = parameters["password"];
            factory.VirtualHost = parameters["virtualhost"];

            var port = 5672;
            int.TryParse(parameters["port"], out port);
            factory.Port = port;

            var endpoints = GetAmqpTcpEndpoint(parameters["host"]);
            return factory.CreateConnection(endpoints);
        }

        private Dictionary<string, string> ParseConnnectionParameter(string connnectionString)
        {
            if (string.IsNullOrEmpty(connnectionString))
                throw new ArgumentNullException(nameof(connnectionString));

            var dict = new Dictionary<string, string>();
            try
            {
                foreach (var item in connnectionString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var pair = item.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    dict.Add(pair[0].ToLower(), pair[1]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(connnectionString, ex);
            }

            return dict;
        }

        private IList<AmqpTcpEndpoint> GetAmqpTcpEndpoint(string hosts)
        {
            if (string.IsNullOrEmpty(hosts))
                throw new ArgumentNullException(nameof(hosts));
            return hosts.Split(',').Select(m => new AmqpTcpEndpoint(new Uri($"amqp://{m}"))).ToList();
        }


        #region Event

        protected void BindChannelEvent(IModel channel)
        {
            channel.BasicAcks += RaiseChannelBasicAcks;
            channel.BasicNacks += RaiseChannelBasicNacks;
            channel.BasicRecoverOk += RaiseChannelBasicRecoverOk;
            channel.BasicReturn += RaiseChannelBasicReturn;
            channel.CallbackException += RaiseChannelCallbackException;
            channel.FlowControl += RaiseChannelFlowControl;
            channel.ModelShutdown += RaiseChannelModelShutdown;
        }

        protected void BindConnectionEvent(IConnection connection)
        {
            connection.CallbackException += RaiseConnnectionCallbackException;
            connection.ConnectionBlocked += RaiseConnectionBlocked;
            connection.ConnectionRecoveryError += RaiseConnectionRecoveryError;
            connection.ConnectionShutdown += RaiseConnectionShutdown;
            connection.ConnectionUnblocked += ConnectionUnblocked;
            connection.RecoverySucceeded += RaiseConnectionRecoverySucceeded;
        }

        public event EventHandler<CallbackExceptionEventArgs> ConnnectionCallbackException;

        protected void RaiseConnnectionCallbackException(object sender, CallbackExceptionEventArgs callbackExceptionEventArgs)
        {
            ConnnectionCallbackException?.Invoke(sender, callbackExceptionEventArgs);
        }


        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;

        protected void RaiseConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            ConnectionBlocked?.Invoke(sender, connectionBlockedEventArgs);
        }

        public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError;
        protected void RaiseConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs connectionRecoveryErrorEventArgs)
        {
            ConnectionRecoveryError?.Invoke(sender, connectionRecoveryErrorEventArgs);
        }

        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
        protected void RaiseConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionShutdown?.Invoke(sender, shutdownEventArgs);
        }

        public event EventHandler<EventArgs> ConnectionUnblocked;
        protected void RaiseConnectionUnblocked(object sender, EventArgs eventArgs)
        {
            ConnectionUnblocked?.Invoke(sender, eventArgs);
        }

        public event EventHandler<EventArgs> ConnectionRecoverySucceeded;
        protected void RaiseConnectionRecoverySucceeded(object sender, EventArgs eventArgs)
        {
            ConnectionRecoverySucceeded?.Invoke(sender, eventArgs);
        }


        public event EventHandler<BasicAckEventArgs> ChannelBasicAcks;
        protected void RaiseChannelBasicAcks(object sender, BasicAckEventArgs basicAckEventArgs)
        {
            ChannelBasicAcks?.Invoke(sender, basicAckEventArgs);
        }

        public event EventHandler<BasicNackEventArgs> ChannelBasicNacks;
        protected void RaiseChannelBasicNacks(object sender, BasicNackEventArgs basicNackEventArgs)
        {
            ChannelBasicNacks?.Invoke(sender, basicNackEventArgs);
        }

        public event EventHandler<EventArgs> ChannelBasicRecoverOk;
        protected void RaiseChannelBasicRecoverOk(object sender, EventArgs eventArgs)
        {
            ChannelBasicRecoverOk?.Invoke(sender, eventArgs);
        }

        public event EventHandler<BasicReturnEventArgs> ChannelBasicReturn;
        protected void RaiseChannelBasicReturn(object sender, BasicReturnEventArgs basicReturnEventArgs)
        {
            ChannelBasicReturn?.Invoke(sender, basicReturnEventArgs);
        }

        public event EventHandler<CallbackExceptionEventArgs> ChannelCallbackException;
        protected void RaiseChannelCallbackException(object sender, CallbackExceptionEventArgs callbackExceptionEventArgs)
        {
            ChannelCallbackException?.Invoke(sender, callbackExceptionEventArgs);
        }

        public event EventHandler<FlowControlEventArgs> ChannelFlowControl;
        protected void RaiseChannelFlowControl(object sender, FlowControlEventArgs flowControlEventArgs)
        {
            ChannelFlowControl?.Invoke(sender, flowControlEventArgs);
        }

        public event EventHandler<ShutdownEventArgs> ChannelModelShutdown;

        protected void RaiseChannelModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ChannelModelShutdown?.Invoke(sender, shutdownEventArgs);
        }

        #endregion
    }
}