using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFoodDelivery.Models.Models;
using Microsoft.Azure.Cosmos;
using System.Threading;
using System.Web.Http;

namespace FunctionApp.Orders
{
    public class OrderFunction
    {
        [FunctionName("GetOrder")]
        public IActionResult GetOrder([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderId}")] HttpRequest req,
             [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{orderId}",
                PartitionKey = "{orderId}")] Order order,
            ILogger log)
        {
            if (order != null)
            {
                return new OkObjectResult(order);
            }
            else
            {
                var result = new ObjectResult($"Order {order.Id} not found");
                result.StatusCode = StatusCodes.Status404NotFound;
                return result;
            }
        }


        [FunctionName("PlaceNewOrder")]
        public async Task<ActionResult> PlaceNewOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")]
        HttpRequest req,
            [ServiceBus("%OrderNewQueue%", Connection = "ServiceBusConnection")]
            IAsyncCollector<dynamic> serviceBusQueue,
           ILogger log)
        {
            try
            {
                log.LogInformation("placing order...");
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                Order order = JsonConvert.DeserializeObject<Order>(requestBody);
                return await AddToQueue(order, serviceBusQueue);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }

        }

        [FunctionName("OrderAccepted")]
        public async Task<IActionResult> OrderAccepted([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/accepted/{orderId}")] HttpRequest req,
            [ServiceBus("%OrderAcceptedQueue%", Connection = "ServiceBusConnection")]
            IAsyncCollector<dynamic> serviceBusQueue,
            [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{orderId}",
                PartitionKey = "{orderId}")] Order order,
        ILogger log)
        {
            try
            {
                return await AddToQueue(order, serviceBusQueue);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [FunctionName("OrderOutForDelivery")]
        public async Task<IActionResult> OrderOutForDelivery([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/outForDelivery/{orderId}")] HttpRequest req,
            [ServiceBus("%OrderOutForDeliveryQueue%", Connection = "ServiceBusConnection")]
        IAsyncCollector<dynamic> serviceBusQueue,
             [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{orderId}",
                PartitionKey = "{orderId}")] Order order,
       ILogger log)
        {
            try
            {
                return await AddToQueue(order, serviceBusQueue);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [FunctionName("OrderDelivered")]
        public async Task<IActionResult> OrderDelivered([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/delivered/{orderId}")] HttpRequest req,
            [ServiceBus("%OrderDeliveredQueue%", Connection = "ServiceBusConnection")]
        IAsyncCollector<dynamic> serviceBusQueue,
              [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{orderId}",
                PartitionKey = "{orderId}")] Order order,
       ILogger log)
        {
            try
            {
                return await AddToQueue(order, serviceBusQueue);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }
        private async Task<ActionResult> AddToQueue(Order order, IAsyncCollector<dynamic> serviceBusQueue)
        {
            if (order != null)
            {
                await serviceBusQueue.AddAsync(order);
                return new OkObjectResult(order);
            }
            else
            {
                var result = new ObjectResult($"Order {order.Id} not found");
                result.StatusCode = StatusCodes.Status404NotFound;
                return result;
            }
        }

    }

}
