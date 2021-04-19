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
using Microsoft.Azure.WebJobs.Host;
using ServerlessFoodDelivery.Shared;
using System.Reflection;

namespace FunctionApp.Orders
{
    public class OrderFunction
    {      

        [FunctionName("GetOrder")]
        public IActionResult GetOrder([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderId}")] HttpRequest req,
             [CosmosDB(
                databaseName: "%CosmosDbFoodDeliveryDatabaseName%",
                collectionName: "%CosmosDbOrderContainerName%",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{orderId}",
                PartitionKey = "{orderId}")] Order order, ILogger log)
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
            IAsyncCollector<dynamic> serviceBusQueue, ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);
            return await AddToQueue(log, order, serviceBusQueue);
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
                PartitionKey = "{orderId}")] Order order, ILogger log)
        {
            return await AddToQueue(log, order, serviceBusQueue);
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
                PartitionKey = "{orderId}")] Order order, ILogger log)
        {
            return await AddToQueue(log, order, serviceBusQueue);
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
                PartitionKey = "{orderId}")] Order order, ILogger log)
        {
            return await AddToQueue(log, order, serviceBusQueue);
        }
        private async Task<ActionResult> AddToQueue(ILogger log, Order order, IAsyncCollector<dynamic> serviceBusQueue)
        {
            if (order != null)
            {
                try
                {
                    await serviceBusQueue.AddAsync(order);                    
                    return new OkObjectResult(order);
                }
                catch(Exception ex)
                {
                    ex.LogExceptionDetails(log, order.Id, GetType().FullName);
                    var result = new ObjectResult($"Something went wrong while processing the order. Message: {ex.Message}");
                    result.StatusCode = StatusCodes.Status500InternalServerError;
                    return result;
                }               
            }
            else
            {
                var result = new ObjectResult($"Order not found");
                result.StatusCode = StatusCodes.Status404NotFound;
                return result;
            }
        }
    }

}
