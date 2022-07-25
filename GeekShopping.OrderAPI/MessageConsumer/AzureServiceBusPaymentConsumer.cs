using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.MessageSender;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Repository;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class AzureServiceBusPaymentConsumer : BackgroundService
    {
        private readonly IQueueClient _queueClient;
        private readonly OrderRepository _repository;

        private const string QueueName = "orderpaymentresultqueue";

        public AzureServiceBusPaymentConsumer(OrderRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _queueClient = new QueueClient(configuration.GetConnectionString("AzureServiceBus"), QueueName);
        }

        ~AzureServiceBusPaymentConsumer()
        {
            _queueClient.CloseAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var messageHandlerOptions = new MessageHandlerOptions(ErrorHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            _queueClient.RegisterMessageHandler(MessageHandler, messageHandlerOptions);
        }

        private async Task UpdatePaymentStatus(UpdatePaymentResultVO vo)
        {
            try
            {
                await _repository.UpdateOrderPaymentStatus(vo.OrderId, vo.Status);
            }
            catch (Exception ex)
            {

                throw new Exception($"Erro ao atualizar o status do pagamento: {ex.Message}");
            }            
        }

        private async Task MessageHandler(Message message, CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var body = Encoding.UTF8.GetString(message.Body.ToArray());
            Console.WriteLine($"Received: {body}");
            UpdatePaymentResultVO vo = JsonSerializer.Deserialize<UpdatePaymentResultVO>(body);
            UpdatePaymentStatus(vo).GetAwaiter().GetResult();
            // complete the message. message is deleted from the queue. 
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ExceptionReceivedEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
