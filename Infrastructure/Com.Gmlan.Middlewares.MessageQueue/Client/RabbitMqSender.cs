using Com.Gmlan.Middlewares.MessageQueue.Model;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Com.Gmlan.Middlewares.MessageQueue.Client
{
    public class RabbitMqSender : RabbitMqClient
    {
        private readonly string _exchage;
        private readonly string _queue;
        private readonly string _routingKey;

        public RabbitMqSender(string connectionString, string queue, string exchage, string routingKey)
            : base(connectionString)
        {
            _queue = queue;
            _exchage = exchage;
            _routingKey = routingKey;
        }

        public RabbitMqSender(string connectionString, string exchage, string routingKey)
            : this(connectionString, string.Empty, exchage, routingKey)
        {
        }

        public void Send(IEnumerable<string> messges, MessagePriority priority = MessagePriority.Normal)
        {
            using (var connection = GetConnection())
            {
                BindConnectionEvent(connection);
                using (var channel = connection.CreateModel())
                {
                    BindChannelEvent(channel);

                    if (!string.IsNullOrEmpty(_queue))
                        channel.QueueDeclare(_queue, true, false, false, Arguments);

                    var headers = channel.CreateBasicProperties();
                    headers.Priority = (byte)priority;
                    foreach (var messge in messges)
                        channel.BasicPublish(_exchage, _routingKey, headers, Encode(messge));
                }
            }
        }

        public void Send(string messge, MessagePriority priority = MessagePriority.Normal)
        {
            Send(new List<string> { messge }, priority);
        }
    }
}