namespace Com.Gmlan.Middlewares.MessageQueue.Event
{
    public class ReceivedEventModel
    {
        public ulong DeliveryTag { get; set; }

        public string Data { get; set; }
    }
}