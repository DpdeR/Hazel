using System;
using System.Collections;
using Hazel.Abstractions;
using Hazel.RabbitMq;
using Hazel.RabbitMq.Receive;
using Hazel.RabbitMq.Send;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hazel
{
    public static class ServiceCollectionExtensions
    {
        private static Pattern _pattern;
        private static Configuration _config;

        public static void UseHazel(this IServiceCollection services, Pattern pattern, IConfiguration configuration)
        {
            _pattern = pattern;
            _config = configuration.GetSection("Hazel").Get<Configuration>();
        }

        public static void AddHazelItem<T>(this IServiceCollection services, string key)
        {
            switch (_pattern)
            {
                case Pattern.Queue:
                    services.AddTransient<IHazelSender<T>, RabbitMqSenderBase<T>>(_ =>
                        new QueueSenderBase<T>(_config, key));
                    break;
                case Pattern.Topic:
                    services.AddTransient<IHazelSender<T>, RabbitMqSenderBase<T>>(_ =>
                        new TopicSenderBase<T>(_config, key));
                    break;
                default:
                    throw new NullReferenceException(
                        "Hazel was not initialized. Make sure to call UseHazel with a pattern and configuration");
            }
        }

        public static void AddHazelListener<T1, T2>(this IServiceCollection services, string key) where T2 : IHazelCallback<T1>
        {
            if(_pattern == Pattern.Undefined) throw new NullReferenceException(
                "Hazel was not initialized. Make sure to call UseHazel with a pattern and configuration");
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(
                    "No keys provided");
            Console.WriteLine("binding with key: " + key);
            if (_pattern == Pattern.Queue)
            {
                services.AddHostedService(
                    serviceProvider =>
                        new QueueListener<T1, T2>(
                            serviceProvider.GetService<IConfiguration>(),
                            serviceProvider,
                            key)
                );
            }
            else
            {
                services.AddHostedService(
                    serviceProvider =>
                        new TopicListener<T1, T2>(
                            serviceProvider.GetService<IConfiguration>(),
                            serviceProvider,
                            key)
                );
            }
        }
    }
}