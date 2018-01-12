using Com.Gmlan.Middlewares.MessageQueue.Event;
using Com.Gmlan.Middlewares.MessageQueue.Extension;
using RabbitMQ.Client.Events;
using System;

namespace Com.Gmlan.Middlewares.MessageQueue.Client.Generic
{
    public class RabbitMqReceiver<T> : RabbitMqReceiver where T : new()
    {
        public RabbitMqReceiver(string connectionString, string queue, bool noAck = false, ushort maxFetchCount = 1)
            : base(connectionString, queue, noAck, maxFetchCount)
        {
        }

        protected override void Deserialize(object obj, string message, BasicDeliverEventArgs e)
        {
            var msg = message.FromJson<T>();

            if (Received != null)
                Received(obj, new ReceivedEventArgs<T>(msg, e, this));
        }

        public new event EventHandler<ReceivedEventArgs<T>> Received;
    }
}