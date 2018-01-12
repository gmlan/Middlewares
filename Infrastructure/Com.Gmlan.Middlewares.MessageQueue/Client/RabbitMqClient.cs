using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Gmlan.Middlewares.MessageQueue.Client
{
    public abstract class RabbitMqClient
    {
        private readonly string _connectionString;

        protected IDictionary<string, object> Arguments = new Dictionary<string, object>();


        protected RabbitMqClient(string connectionString)
        {
            _connectionString = connectionString;
            Arguments["x-max-priority"] = 10;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;


        public byte[] Encode(string message)
        {
            return Encoding.GetBytes(message);
        }

        public string Decode(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }

        //“host=myServer;port=5672;virtualHost=myVirtualHost;username=mike;password=topsecret”

        public IConnection GetConnection()
        {
            var factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ContinuationTimeout = new TimeSpan(5000),
                NetworkRecoveryInterval = new TimeSpan(10000),
                RequestedHeartbeat = 60
            };

            if (_connectionString.ToLower().StartsWith("amqp://"))
            {
                factory.Uri = new Uri(_connectionString);
                return factory.CreateConnection();
            }

            var parameters = ParseConnnectionParameter(_connectionString);
            factory.UserName = parameters["username"];
            factory.Password = parameters["password"];
            factory.VirtualHost = parameters["virtualhost"];

            var port = 5672;
            int.TryParse(parameters["port"], out port);
            factory.Port = port;

            var endpoints = GetAmqpTcpEndpoint(parameters["host"]);
            return factory.CreateConnection(endpoints);
        }

        private Dictionary<string, string> ParseConnnectionParameter(string connnectionString)
        {
            if (string.IsNullOrEmpty(connnectionString))
                throw new ArgumentNullException(nameof(connnectionString));

            var dict = new Dictionary<string, string>();
            try
            {
                foreach (var item in connnectionString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var pair = item.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    dict.Add(pair[0].ToLower(), pair[1]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(connnectionString, ex);
            }

            return dict;
        }

        private IList<AmqpTcpEndpoint> GetAmqpTcpEndpoint(string hosts)
        {
            if (string.IsNullOrEmpty(hosts))
                throw new ArgumentNullException(nameof(hosts));
            return hosts.Split(',').Select(m => new AmqpTcpEndpoint(new Uri($"amqp://{m}"))).ToList();
        }
    }
}