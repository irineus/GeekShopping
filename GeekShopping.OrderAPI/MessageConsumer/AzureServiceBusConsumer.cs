﻿using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Repository;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class AzureServiceBusConsumer : BackgroundService
    {
        private readonly IQueueClient _queueClient;
        private readonly OrderRepository _repository;

        private const string QueueName = "checkoutqueue";

        public AzureServiceBusConsumer(OrderRepository repository, IConfiguration configuration)
        {
            _repository = repository;
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

        private async Task MessageHandler(Message message, CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var body = Encoding.UTF8.GetString(message.Body.ToArray());
            Console.WriteLine($"Received: {body}");
            CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(body);
            ProcessOrder(vo).GetAwaiter().GetResult();
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
