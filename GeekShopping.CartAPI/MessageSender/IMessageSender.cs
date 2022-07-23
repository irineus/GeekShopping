using GeekShopping.MessageBus;

namespace GeekShopping.CartAPI.MessageSender
{
    public interface IMessageSender
    {
        void SendMessageAsync(BaseMessage message, string queueName);
    }
}
