namespace Com.Gmlan.Core.MessageQueue
{
    public interface IMessageQueueConsumer
    {
        void Consume(QueueMessageModel model);
    }
}
