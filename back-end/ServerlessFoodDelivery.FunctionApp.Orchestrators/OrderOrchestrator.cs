using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using ServerlessFoodDelivery.Models.Models;
using ServerlessFoodDelivery.Shared.Helpers;
using ServerlessFoodDelivery.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ServerlessFoodDelivery.Models.Enums;

namespace ServerlessFoodDelivery.FunctionApp.Orchestrators
{
    public class OrderOrchestrator
    {
        
        [FunctionName("OrderPlacedOrchestrator")]
        public static async Task OrderPlacedOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            try
            {
                Order order = context.GetInput<Order>();
                log.LogInformation("Notifying restaurant...");
                await context.CallActivityAsync("NotifyRestaurant", order);
                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddSeconds(60);
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var acknowledgeTask = context.WaitForExternalEvent(Constants.RESTAURANT_ORDER_ACCEPT_EVENT);
                    log.LogInformation("Waiting for restaurant to accept the order...");
                    var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);
                    if (winner == acknowledgeTask)
                    {
                        log.LogInformation("Order accepted event received...");
                        log.LogInformation("Notifying customer...");
                        await context.CallActivityAsync("NotifyCustomer", order);
                        string instanceId = $"{order.Id}-accepted"; 
                        context.StartNewOrchestration("OrderAcceptedOrchestrator", order, instanceId);
                        cts.Cancel(); // we should cancel the timeout task
                    }
                    else
                    {
                        log.LogWarning("Timed out!!!");
                        //TODO: Handle time out logic
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;

            }
          

        }

        [FunctionName("OrderAcceptedOrchestrator")]
        public static async Task OrderAcceptedOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            Order order = context.GetInput<Order>();
            log.LogInformation("Order preparing...");
            using (var cts = new CancellationTokenSource())
            {
                var timeoutAt = context.CurrentUtcDateTime.AddHours(1);
                var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                var acknowledgeTask = context.WaitForExternalEvent(Constants.RESTAURANT_ORDER_OUTFORDELIVERY_EVENT);
                var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);
                if (winner == acknowledgeTask)
                {
                    log.LogInformation("Order is out for delivery event received...");
                   
                    log.LogInformation("Notifying customer...");
                    await context.CallActivityAsync("NotifyCustomer", order);
                    cts.Cancel(); // we should cancel the timeout task
                }
                else
                {
                    await context.CallActivityAsync("NotifyRestaurant", order);
                    //Handle time out logic

                }
            }

        }


        [FunctionName("NotifyRestaurant")]
        public static void NotifyRestaurant([ActivityTrigger] Order order,
            ILogger log)
        {
            //TODO: Send notification to customer
            log.LogInformation("Customer notified");

        }

        [FunctionName("NotifyCustomer")]
        public static void NotifyCustomer([ActivityTrigger] Order order,
              ILogger log)
        {
            //TODO: Send notification to customer
            log.LogInformation("Customer notified");
        }
    }
}
