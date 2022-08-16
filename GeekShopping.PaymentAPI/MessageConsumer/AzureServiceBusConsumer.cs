using Azure.Messaging.ServiceBus;
using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.MessageSender;
using GeekShopping.PaymentProcessor;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    public class AzureServiceBusConsumer : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly IProcessPayment _processPayment;
        private readonly IMessageSender _messageSender;

        private const string QueueName = "orderpaymentprocessqueue";

        public AzureServiceBusConsumer(IProcessPayment processPayment, IConfiguration configuration, IMessageSender messageSender)
        {
            _processPayment = processPayment;
            _messageSender = messageSender;
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

        private async Task<bool> ProcessPayment(PaymentMessage vo)
        {
            try
            {
                var result = _processPayment.PaymentProcessor();
                UpdatePaymentResultMessage paymentResult = new UpdatePaymentResultMessage
                {
                    Status = result,
                    OrderId = vo.OrderId,
                    Email = vo.Email
                };
                await _messageSender.SendMessageAsync(paymentResult, "orderpaymentresultqueue");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");
            PaymentMessage vo = JsonSerializer.Deserialize<PaymentMessage>(body);
            if (ProcessPayment(vo).GetAwaiter().GetResult())
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
