using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazel.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Hazel.RabbitMq.Receive
{
    public abstract class ListenerBase<T, T1> : BackgroundService
    {
        protected readonly string _hostname;
        protected readonly string _username;
        protected readonly string _password;
        protected readonly string _exchange;
        
        protected IConnection _connection;
        protected IModel _channel;

        protected readonly T1 _service;

        protected string _queueName;

        protected ListenerBase(IConfiguration c, IServiceProvider services)
        {
            var x = c.GetSection("Hazel").Get<Configuration>();
            _hostname = x.HostName;
            _username = x.UserName;
            _password = x.Password;
            _exchange = x.Exchange;

            using var scope = services.CreateScope();
            _service =
                scope.ServiceProvider
                    .GetRequiredService<T1>();
            (_service as IHazelCallback<T>).ConfirmReachable();
        }

        protected virtual void Initialize(string key)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        private void HandleCallback(T o, BasicDeliverEventArgs ea)
        {
            Console.WriteLine("callback received");
            (_service as IHazelCallback<T>)?.OnReceiveAsync(o);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            Console.WriteLine("receivers mapped");
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (_, ea) =>
            {
                Console.WriteLine("received");
                var body = ea.Body.ToArray();
                var content = Encoding.UTF8.GetString(body);
                var o = JsonConvert.DeserializeObject<T>(content);
                HandleCallback(o, ea);
            };
            
            _channel.BasicConsume(_queueName, true, consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        { 
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}