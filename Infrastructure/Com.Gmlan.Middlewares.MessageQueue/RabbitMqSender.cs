using RabbitMQ.Client;
using System.Collections.Generic;

namespace Com.Gmlan.Middlewares.MessageQueue
{
    public class RabbitMqSender : RabbitMqClient
    {
        private readonly string _queue;
        private readonly string _exchage;
        private readonly string _routingKey;

        public RabbitMqSender(string uri, string queue, string exchage, string routingKey)
            : base(uri)
        {
            _queue = queue;
            _exchage = exchage;
            _routingKey = routingKey;
        }

        public RabbitMqSender(string uri, string exchage, string routingKey)
            : this(uri, string.Empty, exchage, routingKey)
        {
        }

        public void Send(IEnumerable<string> messges)
        {
            using (var connection = GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    if (!string.IsNullOrEmpty(_queue))
                        channel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    foreach (var messge in messges)
                    {
                        channel.BasicPublish(_exchage, _routingKey, null, Encode(messge));
                    }
                }
            }
        }

        public void Send(string messge)
        {
            Send(new List<string> { messge });
        }
    }
}
