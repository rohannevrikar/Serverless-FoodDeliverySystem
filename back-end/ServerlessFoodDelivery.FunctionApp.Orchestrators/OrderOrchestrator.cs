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
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            try
            {

                Order order = context.GetInput<Order>();
                log.LogInformation(order.Id + " NewOrderOrchestrationStartTime: " + DateTime.UtcNow.ToString());

                await context.CallActivityAsync("UpsertOrder", order);
                log.LogInformation("Order placed...");

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
                        log.LogInformation("Order accepted event received..." + order.Id + " " + DateTime.UtcNow.ToString());
                        string instanceId = $"{order.Id}-accepted";
                        log.LogInformation(instanceId + " AcceptOrderOrchestrationTriggerTime: " + DateTime.UtcNow.ToString());

                        context.StartNewOrchestration("OrderAcceptedOrchestrator", order, instanceId);
                        await context.CallActivityAsync("NotifyCustomer", order);
                        cts.Cancel(); // we should cancel the timeout task

                    }
                    else
                    {
                        log.LogWarning("Timed out!!!");
                        //TODO: Handle time out logic
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        [FunctionName("OrderAcceptedOrchestrator")]
        public static async Task OrderAcceptedOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            Order order = context.GetInput<Order>();

            log.LogInformation(order.Id + " AcceptOrderOrchestrationStartTime: " + DateTime.UtcNow.ToString());
            order.OrderStatus = OrderStatus.Accepted;
            await context.CallActivityAsync("UpsertOrder", order);
            await context.CallActivityAsync("NotifyRestaurant", order);
            log.LogInformation("Order preparing...");



            using (var cts = new CancellationTokenSource())
            {
                log.LogInformation("Instance Id in accepted orchestrator: " + context.InstanceId);
                var timeoutAt = context.CurrentUtcDateTime.AddHours(1);
                var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                var acknowledgeTask = context.WaitForExternalEvent(Constants.RESTAURANT_ORDER_OUTFORDELIVERY_EVENT);
                var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);
                if (winner == acknowledgeTask)
                {
                    log.LogInformation("Order is out for delivery event received..." + order.Id + " " + DateTime.UtcNow.ToString());
                    string instanceId = $"{order.Id}-out-for-delivery";
                    log.LogInformation(instanceId + " OutForDeliveryOrderOrchestrationTriggerTime: " + DateTime.UtcNow.ToString());

                    context.StartNewOrchestration("OrderOutForDeliveryOrchestrator", order, instanceId);
                    await context.CallActivityAsync("NotifyCustomer", order);
                    cts.Cancel(); // we should cancel the timeout task
                }
                else
                {
                    log.LogInformation("Order taking longer than usual");
                    await context.CallActivityAsync("NotifyRestaurant", order);
                    //Handle time out logic

                }
            }
        }

        [FunctionName("OrderOutForDeliveryOrchestrator")]
        public static async Task OrderOutForDeliveryOrchestrator(
     [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            Order order = context.GetInput<Order>();


            log.LogInformation(order.Id + " OutForDeliveryOrderOrchestrationStartTime: " + DateTime.UtcNow.ToString());
            order.OrderStatus = OrderStatus.OutForDelivery;
            await context.CallActivityAsync("UpsertOrder", order);
            await context.CallActivityAsync("NotifyCustomer", order);
            log.LogInformation("Order out for delivery...");



            using (var cts = new CancellationTokenSource())
            {
                log.LogInformation("Instance Id in out for delivery orchestrator: " + context.InstanceId);
                var timeoutAt = context.CurrentUtcDateTime.AddHours(1);
                var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                var acknowledgeTask = context.WaitForExternalEvent(Constants.DELIVERY_ORDER_DELIVERED_EVENT);
                var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);
                if (winner == acknowledgeTask)
                {
                    log.LogInformation("Order is delivered event received..." + order.Id);
                    order.OrderStatus = OrderStatus.Delivered;
                    await context.CallActivityAsync("UpsertOrder", order);
                    await context.CallActivityAsync("NotifyCustomer", order);
                    cts.Cancel(); // we should cancel the timeout task
                }
                else
                {
                    log.LogInformation("Order hasn't been delivered yet...");
                    await context.CallActivityAsync("NotifyRestaurant", order);
                    //Handle time out logic

                }
            }
        }

        [FunctionName("UpsertOrder")]
        public static void UpsertOrder([ActivityTrigger] Order order,
       [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDbConnectionString")] out dynamic document,
       ILogger log)
        {
            document = order;
            //TODO: Send notification to customer
            log.LogInformation(order.Id + " OrderUpserted: " + DateTime.UtcNow.ToString());

        }

        [FunctionName("NotifyRestaurant")]
        public static void NotifyRestaurant([ActivityTrigger] Order order,
            ILogger log)
        {

            //TODO: Send notification to restaurant
            log.LogInformation("Restaurant notified..." + order.Id);

        }

        [FunctionName("NotifyCustomer")]
        public static void NotifyCustomer([ActivityTrigger] Order order,
              ILogger log)
        {
            //TODO: Send notification to customer
            log.LogInformation("Customer notified..." + order.Id);
        }
    }
}
