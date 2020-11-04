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
using Microsoft.Azure.Cosmos;
using System.Threading;

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
                log.LogInformation("Received request...");
                string requestBody = new StreamReader(req.Body).ReadToEnd();                
                Order order = JsonConvert.DeserializeObject<Order>(requestBody);

                if(order != null)
                {
                    log.LogInformation("Received request...");
                    await _storageService.EnqueueNewOrder(order);
                    log.LogInformation("Added to queue...");
                }
                    

                //await _orderService.PlaceNewOrder(order);
                //log.LogInformation("Order placed...");

               

                return new OkObjectResult(order);
            }
            catch(Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
          
        }

        [FunctionName("OrderAccepted")]
        public async Task<IActionResult> OrderAccepted([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/accepted/{orderId}")] HttpRequest req,
             string orderId,
        ILogger log)
        {
            Order order = null;
            try
            {
                order = await _orderService.GetOrder(orderId);
                await _storageService.EnqueueAcceptOrder(order);
                return new OkObjectResult(order);
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.ToString());
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    CosmosException cosmosException = null;
                    log.LogError("Status code is 404");
                    int attempts = 0;
                    while (attempts < 5)
                    {
                        cosmosException = null;
                        Thread.Sleep(3000);
                        log.LogInformation("Attempt count: " + attempts);
                        try
                        {
                            order = await _orderService.GetOrder(orderId);
                            log.LogInformation("Queuing...");
                            await _storageService.EnqueueAcceptOrder(order);
                            return new OkObjectResult(order);

                        }
                        catch (CosmosException cosmosEx)
                        {
                            log.LogError("Caught cosmos exception");
                            cosmosException = cosmosEx;
                        }

                        attempts += 1;
                    }
                    if (cosmosException != null)
                    {
                        throw new Exception("Something went wrong while reading from database");
                    }
                }

            }
           
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        [FunctionName("OrderOutForDelivery")]
        public async Task<IActionResult> OrderOutForDelivery([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/outForDelivery/{orderId}")] HttpRequest req,
            string orderId,
       ILogger log)
        {
            Order order = null;
            try
            {
                order = await _orderService.GetOrder(orderId);
                await _storageService.EnqueueOutForDeliveryOrder(order);
                return new OkObjectResult(order);
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.ToString());
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    CosmosException cosmosException = null;
                    log.LogError("Status code is 404");
                    int attempts = 0;
                    while (attempts < 5)
                    {
                        cosmosException = null;
                        Thread.Sleep(3000);
                        log.LogInformation("Attempt count: " + attempts);
                        try
                        {
                            order = await _orderService.GetOrder(orderId);
                            log.LogInformation("Queuing...");
                            await _storageService.EnqueueOutForDeliveryOrder(order);
                            return new OkObjectResult(order);

                        }
                        catch (CosmosException cosmosEx)
                        {
                            log.LogError("Caught cosmos exception");
                            cosmosException = cosmosEx;
                        }

                        attempts += 1;
                    }
                    if (cosmosException != null)
                    {
                        throw new Exception("Something went wrong while reading from database");
                    }
                }

            }
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        [FunctionName("OrderDelivered")]
        public async Task<IActionResult> OrderDelivered([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/delivered/{orderId}")] HttpRequest req,
            string orderId,
       ILogger log)
        {
            Order order = null;
            try
            {
                order = await _orderService.GetOrder(orderId);
                await _storageService.EnqueueDeliveredOrder(order);
                return new OkObjectResult(order);
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.ToString());
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    CosmosException cosmosException = null;
                    log.LogError("Status code is 404");
                    int attempts = 0;
                    while (attempts < 5)
                    {
                        cosmosException = null;
                        Thread.Sleep(3000);
                        log.LogInformation("Attempt count: " + attempts);
                        try
                        {
                            order = await _orderService.GetOrder(orderId);
                            log.LogInformation("Queuing...");
                            await _storageService.EnqueueDeliveredOrder(order);
                            return new OkObjectResult(order);

                        }
                        catch(CosmosException cosmosEx)
                        {
                            log.LogError("Caught cosmos exception");
                            cosmosException = cosmosEx;
                        }

                        attempts += 1;
                    }
                    if (cosmosException != null)
                    {
                        throw new Exception("Something went wrong while reading from database");
                    }
                }

            }
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
       
    }

}
