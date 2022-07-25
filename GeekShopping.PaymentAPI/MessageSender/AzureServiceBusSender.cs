using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageSender
{
    public class AzureServiceBusSender : IMessageSender
    {
        private readonly IConfiguration _configuration;
    
        public AzureServiceBusSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<bool> SendMessageAsync<T>(T message, string queueName)
        {
            try
            {
                var queueClient = CreateConnection(queueName); 
                var msg = GetMessageAsByteArray(message);
                await queueClient.SendAsync(new Message(msg));
                await CloseConnectionAsync(queueClient);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message '{message}': {ex.Message}");
                return false;
            }
            
        }

        private static async Task CloseConnectionAsync(QueueClient queueClient)
        {
            if (!queueClient.IsClosedOrClosing) await queueClient.CloseAsync();
        }

        private QueueClient CreateConnection(string queueName)
        {
            return new QueueClient(_configuration.GetConnectionString("AzureServiceBus"), queueName);
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
