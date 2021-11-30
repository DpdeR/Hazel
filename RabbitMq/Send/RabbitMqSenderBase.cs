using Hazel.Abstractions;
using RabbitMQ.Client;

namespace Hazel.RabbitMq.Send
{
    public abstract class RabbitMqSenderBase<T> : IHazelSender <T>
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;
        protected readonly string Exchange;


        protected RabbitMqSenderBase(Configuration rabbitMqOptions)
        {
            _hostname = rabbitMqOptions.HostName;
            _username = rabbitMqOptions.UserName;
            _password = rabbitMqOptions.Password;
            Exchange = rabbitMqOptions.Exchange;
            
            CreateConnection();
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
        }
        
        protected bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();

            return _connection != null;
        }

        protected IModel CreateModel()
        {
            return _connection.CreateModel();
        }

        public abstract void Send(T @object);
        public abstract void Send(T @object, string key);
    }
}