using Com.Gmlan.Middlewares.MessageQueue.Event;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Com.Gmlan.Middlewares.MessageQueue.Client
{
    public class RabbitMqReceiver : RabbitMqClient, IDisposable
    {
        private readonly ushort _maxFetchCount;
        private readonly bool _noAck;
        private readonly string _queue;
        private IModel _rabbitMqChannel;
        private IConnection _rabbitMqConnection;

        public RabbitMqReceiver(string connectionString, string queue, bool noAck = false, ushort maxFetchCount = 1) :
            base(connectionString)
        {
            _queue = queue;
            _maxFetchCount = maxFetchCount;
            _noAck = noAck;
        }

        public void Dispose()
        {
            _rabbitMqChannel?.Dispose();
            _rabbitMqConnection?.Dispose();
        }

        public void Start()
        {
            _rabbitMqConnection = GetConnection();
            _rabbitMqChannel = _rabbitMqConnection.CreateModel();

            _rabbitMqChannel.BasicQos(0, _maxFetchCount, true);
            _rabbitMqChannel.QueueDeclare(_queue, true, false, false, Arguments);

            //setup listener
            var consumer = new EventingBasicConsumer(_rabbitMqChannel);
            consumer.Received += ReceivedEvent;
            _rabbitMqChannel.BasicConsume(_queue, _noAck, consumer);
        }

        protected void ReceivedEvent(object obj, BasicDeliverEventArgs e)
        {
            Deserialize(obj, Decode(e.Body), e);
        }

        public void Ack(ulong deliveryTag, bool multiple)
        {
            if (!_noAck)
                _rabbitMqChannel.BasicAck(deliveryTag, multiple);
        }


        protected virtual void Deserialize(object obj, string message, BasicDeliverEventArgs e)
        {
            if (Received != null)
                Received(obj, new ReceivedEventArgs<string>(message, e, this));
        }

        /// <summary>
        /// </summary>
        /// <param name="count"></param>
        /// <param name="noAck">noAck means ack is not neccessary, system will automatically ack the message.</param>
        /// <returns></returns>
        public IList<BasicGetResult> GetQueueMessages(int count, bool noAck)
        {
            if (_rabbitMqConnection == null)
                _rabbitMqConnection = GetConnection();
            if (_rabbitMqChannel == null)
                _rabbitMqChannel = _rabbitMqConnection.CreateModel();
            var messages = new List<BasicGetResult>();
            while (true)
            {
                var message = _rabbitMqChannel.BasicGet(_queue, noAck);
                count--;
                if (message == null)
                    break;
                messages.Add(message);
                if (count == 0)
                    break;
            }

            return messages;
        }

        public event EventHandler<ReceivedEventArgs<string>> Received;
    }
}