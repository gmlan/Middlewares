using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Com.Gmlan.Middlewares.MessageQueue
{
    public class RabbitMqReceiver : RabbitMqClient, IDisposable
    {
        private readonly string _queue;
        private readonly bool _noAck;

        private IConnection _rabbitMqConnection;
        private IModel _rabbitMqChannel;

        public RabbitMqReceiver(string uri, string queue, bool noAck = false) : base(uri)
        {
            _queue = queue;
            _noAck = noAck;
        }

        public void Start()
        {
            if (Received == null)
                throw new NullReferenceException("Received event is not set, no callback available!");

            _rabbitMqConnection = GetConnection();
            _rabbitMqChannel = _rabbitMqConnection.CreateModel();

            //declare queue
            _rabbitMqChannel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            //setup listener
            var consumer = new EventingBasicConsumer(_rabbitMqChannel);
            consumer.Received += Received;
            _rabbitMqChannel.BasicConsume(queue: _queue, noAck: _noAck, consumer: consumer);
        }

        public void Acked(BasicDeliverEventArgs ea)
        {
            _rabbitMqChannel.BasicAck(ea.DeliveryTag, false);
        }

        public event EventHandler<BasicDeliverEventArgs> Received;
        public void Dispose()
        {
            _rabbitMqChannel?.Dispose();
            _rabbitMqConnection?.Dispose();
        }
    }
}
