using System;

namespace Com.Gmlan.Core.MessageQueue
{
    public interface IMessageQueueProducer
    {
        void Produce(Action<object> action);
    }
}
