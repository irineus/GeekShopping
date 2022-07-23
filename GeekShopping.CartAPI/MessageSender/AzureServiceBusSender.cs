using Azure.Messaging.ServiceBus;
using GeekShopping.CartAPI.Messages;
using GeekShopping.MessageBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.CartAPI.MessageSender
{
    public class AzureServiceBusSender : IMessageSender
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public AzureServiceBusSender(IConfiguration configuration)
        {
            _client = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"));
            _sender = _client.CreateSender("checkoutqueue");
        }
        
        ~AzureServiceBusSender()
        {
            _ = _sender.DisposeAsync();
            _ = _client.DisposeAsync();
        }

        public void SendMessageAsync(BaseMessage message, string queueName)
        {
            var jsonMsg = GetMessageAsByteArray(message);
            ServiceBusMessage busMessage = new ServiceBusMessage(jsonMsg);
            try
            {
                _sender.SendMessageAsync(busMessage).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message '{message}': {ex.Message}");
            }
            
        }

        private static byte[] GetMessageAsByteArray(BaseMessage message)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var json = JsonSerializer.Serialize<CheckoutHeaderVO>((CheckoutHeaderVO)message, options);
            var body = Encoding.UTF8.GetBytes(json);
            return body;
        }
    }
}
