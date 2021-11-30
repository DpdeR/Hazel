namespace Hazel.Abstractions
{
    public interface IHazelSender<in T>
    {
        void Send(T @object);
        void Send(T @object, string key);
    }
}