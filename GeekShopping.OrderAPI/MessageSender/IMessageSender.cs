namespace GeekShopping.OrderAPI.MessageSender
{
    public interface IMessageSender
    {
        void SendMessageAsync<T>(T message, string queueName);
    }
}
