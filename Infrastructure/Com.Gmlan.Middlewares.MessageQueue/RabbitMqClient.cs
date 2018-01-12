using RabbitMQ.Client;
using System.Text;

namespace Com.Gmlan.Middlewares.MessageQueue
{
    public abstract class RabbitMqClient
    {
        private readonly string _uri;

        public Encoding Encoding { get; set; } = Encoding.UTF8;


        public byte[] Encode(string message)
        {
            return Encoding.GetBytes(message);
        }

        public string Decode(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }


        protected RabbitMqClient(string uri)
        {

            _uri = uri;
        }

        public IConnection GetConnection()
        {
            return new ConnectionFactory { Uri = _uri }.CreateConnection();
        }
    }
}
