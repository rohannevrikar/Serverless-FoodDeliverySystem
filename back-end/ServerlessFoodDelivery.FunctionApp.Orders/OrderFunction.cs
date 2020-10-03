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
        public OrderFunction(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [FunctionName("GetOrder")]
        public async Task<IActionResult> GetOrder([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderId}")] HttpRequest req,
            string orderId,
            ILogger log)
        {
            return new OkObjectResult(await _orderService.GetOrder(orderId));
        }

        [FunctionName("UpsertOrder")]
        public async Task<IActionResult> UpsertOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest req,
           ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);
            return new OkObjectResult(await _orderService.UpsertOrder(order));
        }

        [FunctionName("OrderAccepted")]
        public async Task<IActionResult> OrderAccepted([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/accepted/{orderId}")] HttpRequest req,
             string orderId,
        ILogger log)
        {
            Order order = await _orderService.GetOrder(orderId);
            return new OkObjectResult(order);
        }

        [FunctionName("OrderOutForDelivery")]
        public async Task<IActionResult> OrderOutForDelivery([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/outForDelivery/{orderId}")] HttpRequest req,
            string orderId,
       ILogger log)
        {
            Order order = await _orderService.GetOrder(orderId);
            return new OkObjectResult(order);
        }

        [FunctionName("OrderDelivered")]
        public async Task<IActionResult> OrderDelivered([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/delivered/{orderId}")] HttpRequest req,
            string orderId,
       ILogger log)
        {
            Order order = await _orderService.GetOrder(orderId);
            return new OkObjectResult(order);
        }

    }
}
