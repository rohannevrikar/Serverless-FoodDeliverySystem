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
                string requestBody = new StreamReader(req.Body).ReadToEnd();                
                Order order = JsonConvert.DeserializeObject<Order>(requestBody);
               


                if (order != null)
                {
                    log.LogInformation(order.Id + " Received request... " + DateTime.UtcNow.ToString());
                    await _storageService.EnqueueNewOrder(order);
                    log.LogInformation(order.Id + " Added to queue... " + DateTime.UtcNow.ToString());
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
                log.LogInformation(orderId + " AcceptOrderAPICallTime: " + DateTime.UtcNow.ToString());
                order = await _orderService.GetOrder(orderId);                
                await _storageService.EnqueueAcceptOrder(order);
                log.LogInformation(orderId + " accepted order added to queue... " + DateTime.UtcNow.ToString());
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
                        Thread.Sleep(5000);
                        log.LogInformation(orderId + " Attempt count: " + attempts);
                        try
                        {
                            order = await _orderService.GetOrder(orderId);
                            log.LogInformation(orderId + "Queuing to accept order...");
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
                        throw new Exception(orderId + " Something went wrong while reading from database: " + cosmosException.Message);
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
                log.LogInformation(orderId + " OutForDeliveryOrderAPICallTime: " + DateTime.UtcNow.ToString());
                order = await _orderService.GetOrder(orderId);
                await _storageService.EnqueueOutForDeliveryOrder(order);
                log.LogInformation(orderId + " out for delivery order added to queue... " + DateTime.UtcNow.ToString());
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
                        Thread.Sleep(5000);
                        log.LogInformation(orderId + " Attempt count: " + attempts);
                        try
                        {
                            order = await _orderService.GetOrder(orderId);
                            log.LogInformation(orderId + "Queuing to out for delivery order...");
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
                        throw new Exception(orderId + " Something went wrong while reading from database: " + cosmosException.Message);
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
                log.LogInformation(orderId + " DeliveredOrderAPICallTime: " + DateTime.UtcNow.ToString());
                order = await _orderService.GetOrder(orderId);
                await _storageService.EnqueueDeliveredOrder(order);
                log.LogInformation(orderId + " delivered order added to queue... " + DateTime.UtcNow.ToString());

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
                        Thread.Sleep(5000);
                        log.LogInformation(orderId + "Attempt count: " + attempts);
                        try
                        {
                            order = await _orderService.GetOrder(orderId);
                            log.LogInformation(orderId + "Queuing to delivered order...");
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
                        throw new Exception(orderId + " Something went wrong while reading from database: " + cosmosException.Message);
                    }
                }

            }
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
       
    }

}
