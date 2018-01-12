using Com.Gmlan.Middlewares.MessageQueue.Client;
using RabbitMQ.Client.Events;
using System;

namespace Com.Gmlan.Middlewares.MessageQueue.Event
{
    public class ReceivedEventArgs<T> : EventArgs
    {
        public ReceivedEventArgs(T val, BasicDeliverEventArgs deliverEventArgs, RabbitMqReceiver receiver)
        {
            Receiver = receiver;
            Value = val;
            DeliverEventArgs = deliverEventArgs;
        }

        public RabbitMqReceiver Receiver { get; set; }

        public T Value { get; set; }

        public BasicDeliverEventArgs DeliverEventArgs { get; set; }
    }
}