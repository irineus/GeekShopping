using Azure.Messaging.ServiceBus;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Repository;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class AzureServiceBusConsumerAlternative : BackgroundService
    {
        private readonly OrderRepository _repository;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public AzureServiceBusConsumerAlternative(OrderRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _client = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"));
            _processor = _client.CreateProcessor("checkoutqueue", new ServiceBusProcessorOptions());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task ProcessOrder(CheckoutHeaderVO vo)
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
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            Console.WriteLine($"Received: {args.Message.Body}");
            var body = Encoding.UTF8.GetString(args.Message.Body.ToArray());
            CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(body);
            ProcessOrder(vo).GetAwaiter().GetResult();
            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
