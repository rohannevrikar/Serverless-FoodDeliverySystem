using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFoodDelivery.Shared.Services;
using ServerlessFoodDelivery.Models.Models;

namespace FunctionApp.Orders
{
    public class OrderFunction
    {
        private readonly IOrderService _orderService;
        private readonly IStorageService _storageService;
        public OrderFunction(IOrderService orderService, IStorageService storageService)
        {
            _storageService = storageService;
            _orderService = orderService;
        }

        [FunctionName("GetOrder")]
        public async Task<IActionResult> GetOrder([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderId}")] HttpRequest req,
            string orderId,
            ILogger log)
        {
            return new OkObjectResult(await _orderService.GetOrder(orderId));
        }


        [FunctionName("PlaceNewOrder")]
        public async Task<IActionResult> PlaceNewOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest req,
           ILogger log)
        {
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                Order order = JsonConvert.DeserializeObject<Order>(requestBody);
                await _orderService.PlaceNewOrder(order);
                await _storageService.EnqueueOrderForStatusUpdate(order);
                return new OkObjectResult(order);
            }
            catch(Exception ex)
            {
                throw ex;
            }
          
        }

        [FunctionName("OrderAccepted")]
        public async Task<IActionResult> OrderAccepted([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/accepted/{orderId}")] HttpRequest req,
             string orderId,
        ILogger log)
        {
            Order order = await _orderService.GetOrder(orderId);
            order.OrderStatus = ServerlessFoodDelivery.Models.Enums.OrderStatus.Accepted;
            await _orderService.UpdateOrder(order);

            await _storageService.EnqueueOrderForStatusUpdate(order);
            return new OkObjectResult(order);
        }

        [FunctionName("OrderOutForDelivery")]
        public async Task<IActionResult> OrderOutForDelivery([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/outForDelivery/{orderId}")] HttpRequest req,
            string orderId,
       ILogger log)
        {
            Order order = await _orderService.GetOrder(orderId);
            order.OrderStatus = ServerlessFoodDelivery.Models.Enums.OrderStatus.OutForDelivery;
            await _orderService.UpdateOrder(order);
            await _storageService.EnqueueOrderForStatusUpdate(order);
            return new OkObjectResult("Ok");
        }

        [FunctionName("OrderDelivered")]
        public async Task<IActionResult> OrderDelivered([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/delivered/{orderId}")] HttpRequest req,
            string orderId,
       ILogger log)
        {
            Order order = await _orderService.GetOrder(orderId);
            await _storageService.EnqueueOrderForStatusUpdate(order);
            return new OkObjectResult(order);
        }

    }
}
