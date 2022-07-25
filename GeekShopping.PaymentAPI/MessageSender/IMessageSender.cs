namespace GeekShopping.PaymentAPI.MessageSender
{
    public interface IMessageSender
    {
        Task<bool> SendMessageAsync<T>(T message, string queueName);
    }
}
