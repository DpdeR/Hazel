using RabbitMQ.Client.Events;

namespace Hazel.Abstractions
{
    public interface IHazelCallback<in T>
    {
        public void OnReceiveAsync(T @object);
        public void ConfirmReachable();
    }
}