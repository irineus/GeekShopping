using Azure.Messaging.ServiceBus;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.MessageSender;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Repository;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class AzureServiceBusCheckoutConsumer : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly OrderRepository _repository;
        private readonly IMessageSender _messageSender;

        private const string QueueName = "checkoutqueue";

        public AzureServiceBusCheckoutConsumer(OrderRepository repository, IConfiguration configuration, IMessageSender messageSender)
        {
            _repository = repository;
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

        private async Task<bool> ProcessOrder(CheckoutHeaderVO vo)
        {
            try
            {
                OrderHeader order = new()
                {
                    UserId = vo.UserId,
                    FirstName = vo.FirstName,
                    LastName = vo.LastName,
                    OrderDetails = new List<OrderDetail>(),
                    CardNumber = vo.CardNumber,
                    CouponCode = vo.CouponCode,
                    CVV = vo.CVV,
                    DiscountAmount = vo.DiscountAmount,
                    Email = vo.Email,
                    ExpiryMonthYear = vo.ExpiryMonthYear,
                    OrderTime = DateTime.Now,
                    PurchaseAmount = vo.PurchaseAmount,
                    PaymentStatus = false,
                    Phone = vo.Phone,
                    DateTime = vo.DateTime
                };

                foreach (var details in vo.CartDetails)
                {
                    OrderDetail detail = new()
                    {
                        ProductId = details.ProductId,
                        ProductName = details.Product.Name,
                        Price = details.Product.Price,
                        Count = details.Count,
                    };
                    order.CartTotalItems += details.Count;
                    order.OrderDetails.Add(detail);
                }

                await _repository.AddOrder(order);

                PaymentVO payment = new()
                {
                    Name = order.FirstName + " " + order.LastName,
                    CardNumber = order.CardNumber,
                    CVV = order.CVV,
                    ExpiryMonthYear = order.ExpiryMonthYear,
                    OrderId = order.Id,
                    PurchaseAmount = order.PurchaseAmount,
                    Email = order.Email
                };

                await _messageSender.SendMessageAsync(payment, "orderpaymentprocessqueue");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a ordem: {ex.Message}");
                return false;
            }
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");
            CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(body);
            if (ProcessOrder(vo).GetAwaiter().GetResult())
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
