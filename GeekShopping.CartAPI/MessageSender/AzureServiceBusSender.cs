using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.CartAPI.MessageSender
{
    public class AzureServiceBusSender : IMessageSender
    {
        private readonly IConfiguration _configuration;

        public AzureServiceBusSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendMessageAsync<T>(T message, string queueName)
        {
            try
            {
                var client = CreateConnection(_configuration.GetConnectionString("AzureServiceBus"));
                var sender = client.CreateSender(queueName);
                try
                {
                    await sender.SendMessageAsync(GetMessageAsServiceBusMessage(message));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    await CloseConnectionAsync(client, sender);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message '{message}': {ex.Message}");
            }
            
        }

        private static ServiceBusMessage GetMessageAsServiceBusMessage<T>(T message)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var json = JsonSerializer.Serialize<T>((T)message, options);
            return new ServiceBusMessage(json)
            {
                ContentType = "application/json"
            };
        }

        private static async Task CloseConnectionAsync(ServiceBusClient client, ServiceBusSender sender)
        {
            if (!client.IsClosed)
            {
                await sender.CloseAsync();
            }
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }

        private static ServiceBusClient CreateConnection(string connectionString)
        {
            return new ServiceBusClient(connectionString);
        }
    }
}
