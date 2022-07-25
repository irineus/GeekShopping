using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.MessageSender;
using GeekShopping.PaymentProcessor;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    public class AzureServiceBusConsumer : BackgroundService
    {
        private readonly IQueueClient _queueClient;
        private readonly IProcessPayment _processPayment;
        private readonly IMessageSender _messageSender;

        private const string QueueName = "orderpaymentprocessqueue";

        public AzureServiceBusConsumer(IProcessPayment processPayment, IConfiguration configuration, IMessageSender messageSender)
        {
            _processPayment = processPayment;
            _messageSender = messageSender;
            _queueClient = new QueueClient(configuration.GetConnectionString("AzureServiceBus"), QueueName);
        }

        ~AzureServiceBusConsumer()
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

        private async Task<bool> ProcessPayment(PaymentMessage vo)
        {
            try
            {
                var result = _processPayment.PaymentProcessor();
                UpdatePaymentResultMessage payentResult = new UpdatePaymentResultMessage
                {
                    Status = result,
                    OrderId = vo.OrderId,
                    Email = vo.Email
                };
                return await _messageSender.SendMessageAsync(payentResult, "orderpaymentresultqueue");
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task MessageHandler(Message message, CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var body = Encoding.UTF8.GetString(message.Body.ToArray());
            Console.WriteLine($"Received: {body}");
            PaymentMessage vo = JsonSerializer.Deserialize<PaymentMessage>(body);
            if (ProcessPayment(vo).GetAwaiter().GetResult())
            {
                // complete the message. message is deleted from the queue. 
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            await _queueClient.AbandonAsync(message.SystemProperties.LockToken);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ExceptionReceivedEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
