namespace Com.Gmlan.Middlewares.MessageQueue.Model
{
    /// <summary>
    ///     Class used to wrap queue message and queue metadata
    /// </summary>
    public class QueueMessageModel
    {

        public string QueueName { get; set; }


        public object Data { get; set; }
    }
}
