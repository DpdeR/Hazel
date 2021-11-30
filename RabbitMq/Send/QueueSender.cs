using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Hazel.RabbitMq.Send
{
    public class QueueSenderBase<T> : RabbitMqSenderBase<T>
    {
        private readonly string _queue;
        
        public QueueSenderBase(Configuration rabbitMqOptions, string queue) : base(rabbitMqOptions)
        {
            _queue = queue;
        }

        public override void Send(T @object)
        {
            Send(@object, _queue);
        }

        public override void Send(T @object, string key)
        {
            if (!ConnectionExists()) return;
            using var channel = CreateModel();

            channel.ExchangeDeclare(exchange: Exchange,
                type: ExchangeType.Direct);

            channel.QueueDeclare(
                queue: key,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var json = JsonConvert.SerializeObject(@object);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(
                exchange: Exchange,
                routingKey: key,
                basicProperties: null,
                body: body);
            Console.WriteLine("published " + key);
        }
    }
}