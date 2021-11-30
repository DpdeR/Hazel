using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Hazel.RabbitMq.Send
{
    public class TopicSenderBase<T> : RabbitMqSenderBase<T>
    {
        private readonly string _defaultTopic;
        
        public TopicSenderBase(Configuration rabbitMqOptions, string topic) : base(rabbitMqOptions)
        {
            _defaultTopic = topic;
        }

        public override void Send(T @object)
        {
            Send(@object, _defaultTopic);
        }

        public override void Send(T @object, string key)
        {
   
            if (!ConnectionExists()) return;
            using var channel = CreateModel();

            channel.ExchangeDeclare(exchange: Exchange,
                type: ExchangeType.Topic);

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