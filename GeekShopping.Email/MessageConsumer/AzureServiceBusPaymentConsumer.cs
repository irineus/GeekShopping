using Azure.Messaging.ServiceBus;
using GeekShopping.Email.Messages;
using GeekShopping.Email.Repository;
using System.Text.Json;

namespace GeekShopping.Email.MessageConsumer
{
    public class AzureServiceBusPaymentConsumer : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly EmailRepository _repository;

        private const string TopicName = "paymentupdatetopic";
        private const string SubscriptionName = "paymentupdateemail";

        public AzureServiceBusPaymentConsumer(EmailRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _client = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"));
            _processor = _client.CreateProcessor(TopicName, SubscriptionName, new ServiceBusProcessorOptions()
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

        private async Task<bool> ProcessLogs(UpdatePaymentResultMessage message)
        {
            try
            {
                await _repository.ProcessEmail(message);
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
            UpdatePaymentResultMessage mesage = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(body);
            if (ProcessLogs(mesage).GetAwaiter().GetResult())
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
