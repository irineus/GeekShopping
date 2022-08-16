namespace GeekShopping.OrderAPI.MessageSender
{
    public interface IMessageSender
    {
        Task SendMessageAsync<T>(T message, string queueName);
    }
}
