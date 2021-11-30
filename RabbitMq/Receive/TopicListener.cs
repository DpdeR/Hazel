using System;
using Hazel.Abstractions;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Hazel.RabbitMq.Receive
{
    public class TopicListener<T, T1> : ListenerBase<T, T1>
    {
        public TopicListener(IConfiguration x,  IServiceProvider p, string key) : base(x, p)
        {
            Initialize(key);
        }

        protected sealed override void Initialize(string key)
        {
            base.Initialize(key);
            Console.WriteLine($"initializing with {_exchange}, {_queueName}, {_channel.ChannelNumber}, {key}");
            _channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Topic);
            _queueName = _channel.QueueDeclare().QueueName;
            
            _channel.QueueBind(
                queue: _queueName,
                exchange: _exchange,
                routingKey: key);
        }
    }
}