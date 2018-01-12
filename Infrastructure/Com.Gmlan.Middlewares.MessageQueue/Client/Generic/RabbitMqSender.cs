using Com.Gmlan.Middlewares.MessageQueue.Extension;
using Com.Gmlan.Middlewares.MessageQueue.Model;
using System.Collections.Generic;
using System.Linq;

namespace Com.Gmlan.Middlewares.MessageQueue.Client.Generic
{
    public class RabbitMqSender<T> : RabbitMqSender where T : new()
    {
        public RabbitMqSender(string connectionString, string queue, string exchage, string routingKey)
            : base(connectionString, queue, exchage, routingKey)
        {
        }

        public RabbitMqSender(string connectionString, string exchage, string routingKey)
            : base(connectionString, exchage, routingKey)
        {
        }


        public void Send(IEnumerable<T> messges, MessagePriority priority = MessagePriority.Normal)
        {
            Send(messges.Select(m => m.ToJson()), priority);
        }

        public void Send(T messge, MessagePriority priority = MessagePriority.Normal)
        {
            Send(new List<T> { messge }, priority);
        }
    }
}