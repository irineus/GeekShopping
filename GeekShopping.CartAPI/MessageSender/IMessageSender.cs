using GeekShopping.MessageBus;

namespace GeekShopping.CartAPI.MessageSender
{
    public interface IMessageSender
    {
        void SendMessageAsync<T>(T message, string queueName);
    }
}
