using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.CartAPI.MessageSender
{
    public class AzureServiceBusSender : IMessageSender
    {
        public IConfiguration _configuration { get; }

        public AzureServiceBusSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async void SendMessageAsync<T>(T message, string queueName)
        {
            try
            {
                var queueClient = new QueueClient(_configuration.GetConnectionString("AzureServiceBus"), queueName);
                var msg = GetMessageAsByteArray(message);
                await queueClient.SendAsync(new Message(msg));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message '{message}': {ex.Message}");
            }
            
        }

        private static byte[] GetMessageAsByteArray<T>(T message)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var json = JsonSerializer.Serialize<T>((T)message, options);
            var body = Encoding.UTF8.GetBytes(json);
            return body;
        }
    }
}
