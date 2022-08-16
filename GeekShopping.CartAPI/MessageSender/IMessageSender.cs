using GeekShopping.MessageBus;

namespace GeekShopping.CartAPI.MessageSender
{
    public interface IMessageSender
    {
        Task SendMessageAsync<T>(T message, string queueName);
    }
}
