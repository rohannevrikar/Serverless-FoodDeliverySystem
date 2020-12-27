using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ServerlessFoodDelivery.Models.Models;
using ServerlessFoodDelivery.Shared.Helpers;
using ServerlessFoodDelivery.Shared.Services;

namespace ServerlessFoodDelivery.FunctionApp.Orchestrators
{
    public class OrderOrchestratorTrigger
    {

        [FunctionName("PlaceNewOrderQueueTrigger")]
        public static async Task PlaceNewOrderQueueTrigger(
           [DurableClient] IDurableOrchestrationClient context,
           [QueueTrigger("%OrderNewQueue%", Connection = "AzureWebJobsStorage")] Order order,
           ILogger log)
        {
            try
            {
                log.LogInformation(order.Id + " Queue triggered, new order... " + DateTime.UtcNow.ToString());
                var instanceId = order.Id;
                log.LogInformation(instanceId + " NewOrderOrchestrationTriggerTime: " + DateTime.UtcNow.ToString());
                await StartInstance(context, order, instanceId, log);
            }
            catch (Exception ex)
            {
                var error = $"StartTripMonitorViaQueueTrigger failed: {ex.Message}";
                log.LogError(error);
            }
        }
        [FunctionName("AcceptedOrderQueueTrigger")]
        public static async Task AcceptedOrderQueueTrigger(
        [DurableClient] IDurableOrchestrationClient context,
        [QueueTrigger("%OrderAcceptedQueue%", Connection = "AzureWebJobsStorage")] Order order,
        ILogger log)
        {
            try
            {
                log.LogInformation("Raising accepted event for instance..." + order.Id + " " + DateTime.UtcNow.ToString());
                await context.RaiseEventAsync(order.Id, Constants.RESTAURANT_ORDER_ACCEPT_EVENT);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Instance not found: " + ex.ToString());

                ArgumentException argumentException = null;
                int attempts = 0;
                while (attempts < 5)
                {
                    argumentException = null;
                    Thread.Sleep(5000);
                    log.LogInformation("Attempt count: " + attempts);
                    try
                    {

                        var reportStatus = await context.GetStatusAsync(order.Id);
                        if(reportStatus.RuntimeStatus == OrchestrationRuntimeStatus.Running)
                        {
                            await context.RaiseEventAsync(order.Id, Constants.RESTAURANT_ORDER_ACCEPT_EVENT);
                            break;
                        }                           
                     

                    }
                    catch (ArgumentException argException)
                    {
                        log.LogError("Caught argument exception");
                        argumentException = argException;
                    }

                    attempts += 1;
                }
                
                if (argumentException != null)
                {
                    throw new Exception("Something went wrong while raising event: " + argumentException.Message);
                }
            }
          


        }

        [FunctionName("OutForDeliveryOrderQueueTrigger")]
        public static async Task OutForDeliveryOrderQueueTrigger(
       [DurableClient] IDurableOrchestrationClient context,
       [QueueTrigger("%OrderOutForDeliveryQueue%", Connection = "AzureWebJobsStorage")] Order order,
       ILogger log)
        {
            string instanceId = $"{order.Id}-accepted";
            try
            {
                log.LogInformation("Raising out for delivery event for instance..." + instanceId + " " + DateTime.UtcNow.ToString());
                await context.RaiseEventAsync(instanceId, Constants.RESTAURANT_ORDER_OUTFORDELIVERY_EVENT);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Instance not found: " + ex.ToString());

                ArgumentException argumentException = null;
                int attempts = 0;
                while (attempts < 5)
                {
                    argumentException = null;
                    Thread.Sleep(5000);
                    log.LogInformation("Attempt count: " + attempts);
                    try
                    {

                        var reportStatus = await context.GetStatusAsync(order.Id);
                        if (reportStatus.RuntimeStatus == OrchestrationRuntimeStatus.Running)
                        {
                            await context.RaiseEventAsync(instanceId, Constants.RESTAURANT_ORDER_OUTFORDELIVERY_EVENT);
                            break;
                        }

                    }               
                    catch (ArgumentException argException)
                    {
                        log.LogError("Caught argument exception");
                        argumentException = argException;
                    }

                    attempts += 1;
                }
                if (argumentException != null)
                {
                    throw new Exception("Something went wrong while raising event: " + argumentException.Message);
                }


            }

        }

        [FunctionName("DeliveredOrderQueueTrigger")]
        public static async Task DeliveredOrderQueueTrigger(
     [DurableClient] IDurableOrchestrationClient context,
     [QueueTrigger("%OrderDeliveredQueue%", Connection = "AzureWebJobsStorage")] Order order,
     ILogger log)
        {
            string instanceId = $"{order.Id}-out-for-delivery";

            try
            {
                log.LogInformation("Raising delivered event for instance..." + instanceId + " " + DateTime.UtcNow.ToString());
                await context.RaiseEventAsync(instanceId, Constants.DELIVERY_ORDER_DELIVERED_EVENT);
            }
            catch (ArgumentException ex)
            {
                log.LogError("Instance not found: " + ex.ToString());

                ArgumentException argumentException = null;
                int attempts = 0;
                while (attempts < 5)
                {
                    argumentException = null;
                    Thread.Sleep(5000);
                    log.LogInformation("Attempt count: " + attempts);
                    try
                    {

                        var reportStatus = await context.GetStatusAsync(order.Id);
                        if (reportStatus.RuntimeStatus == OrchestrationRuntimeStatus.Running)
                        {
                            await context.RaiseEventAsync(instanceId, Constants.DELIVERY_ORDER_DELIVERED_EVENT);
                            break;
                        }


                    }                  
                    catch (ArgumentException argException)
                    {
                        log.LogError("Caught argument exception");
                        argumentException = argException;
                    }

                    attempts += 1;
                }
                if (argumentException != null)
                {
                    throw new Exception("Something went wrong while raising event: " + argumentException.Message);
                }


            }
        }

        private static async Task StartInstance(IDurableOrchestrationClient context, Order order, string instanceId, ILogger log)
        {
            try
            {
                var reportStatus = await context.GetStatusAsync(instanceId);
                string runningStatus = reportStatus == null ? "NULL" : reportStatus.RuntimeStatus.ToString();
                log.LogInformation($"Instance running status: '{runningStatus}'.");

                if (reportStatus == null || reportStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
                {
                    await context.StartNewAsync("OrderPlacedOrchestrator", instanceId, order);
                    log.LogInformation($"Started a new instance = '{instanceId}'.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}