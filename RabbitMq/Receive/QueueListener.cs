using System;
using Hazel.Abstractions;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Hazel.RabbitMq.Receive
{
    public class QueueListener<T, T1> : ListenerBase<T, T1>
    {
        public QueueListener(IConfiguration x, IServiceProvider p, string key) : base(x, p)
        {
            Initialize(key);
        }
        
        protected sealed override void Initialize(string key)
        {
            base.Initialize(key);

            _channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Direct);
            
            _queueName = key;
            _channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            Console.WriteLine("listening to " + key + "("+_queueName+")");
        }
    }
}