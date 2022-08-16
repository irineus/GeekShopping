using Azure.Messaging.ServiceBus;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Repository;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class AzureServiceBusPaymentConsumer : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly OrderRepository _repository;

        private const string QueueName = "orderpaymentresultqueue";

        public AzureServiceBusPaymentConsumer(OrderRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _client = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"));
            _processor = _client.CreateProcessor(QueueName, new ServiceBusProcessorOptions()
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                stoppingToken.ThrowIfCancellationRequested();

                _processor.ProcessMessageAsync += MessageHandler;

                _processor.ProcessErrorAsync += ErrorHandler;

                await _processor.StartProcessingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        private async Task<bool> UpdatePaymentStatus(UpdatePaymentResultVO vo)
        {
            try
            {
                await _repository.UpdateOrderPaymentStatus(vo.OrderId, vo.Status);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar o status do pagamento: {ex.Message}");
                return false;
            }            
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");
            UpdatePaymentResultVO vo = JsonSerializer.Deserialize<UpdatePaymentResultVO>(body);
            if (UpdatePaymentStatus(vo).GetAwaiter().GetResult())
            {
                // complete the message. message is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            }
            else
            {
                await args.AbandonMessageAsync(args.Message);
            }

        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
