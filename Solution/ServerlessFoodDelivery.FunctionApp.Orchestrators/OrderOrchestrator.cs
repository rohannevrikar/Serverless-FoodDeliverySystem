using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFoodDelivery.Models.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static ServerlessFoodDelivery.Models.Enums;
using ServerlessFoodDelivery.Shared;
using Microsoft.Azure.WebJobs.Host;

namespace ServerlessFoodDelivery.FunctionApp.Orchestrators
{
    public class OrderOrchestrator
    {
        private readonly IConfiguration _configuration;
        public OrderOrchestrator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Orchestrator functions
        [FunctionName("OrderPlacedOrchestrator")]
        public async Task OrderPlacedOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {

            log = context.CreateReplaySafeLogger(log); //to prevent logging at the time of function replay
            Order order = context.GetInput<Order>();

            try
            {
                await context.CallActivityAsync("UpsertOrder", order);

                await context.CallActivityAsync("NotifyRestaurant", order);

                Uri uri = new Uri($"{_configuration["HostEndpoint"]}/orders/accepted/{order.Id}");

                await context.CallHttpAsync(HttpMethod.Get, uri);

                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddSeconds(60);
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var acknowledgeTask = context.WaitForExternalEvent(Constants.RESTAURANT_ORDER_ACCEPT_EVENT);
                    var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);

                    if (winner == acknowledgeTask)
                    {
                        string instanceId = $"{order.Id}-accepted";

                        context.StartNewOrchestration("OrderAcceptedOrchestrator", order, instanceId);                        

                        cts.Cancel();
                    }
                    else
                    {
                        //TODO: Handle time out logic
                        log.LogError($"OrderPlacedOrchestrator Timed out {order.Id}");

                        await context.CallActivityAsync("NotifyCustomer", order);

                        await context.CallActivityAsync("NotifyRestaurant", order);                       
                    }
                }
            }
            catch (Exception ex)
            {
                if(order != null)
                {
                    ex.LogExceptionDetails(log, order.Id, GetType().FullName);
                }
                else
                {
                    ex.LogExceptionDetails(log, null, GetType().FullName);
                }
            }
        }

        [FunctionName("OrderAcceptedOrchestrator")]
        public async Task OrderAcceptedOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log = context.CreateReplaySafeLogger(log); //to prevent logging at the time of function replay
            Order order = context.GetInput<Order>();

            try
            {
                order.OrderStatus = OrderStatus.Accepted;

                await context.CallActivityAsync("UpsertOrder", order);

                await context.CallActivityAsync("NotifyCustomer", order);

                Uri uri = new Uri($"{_configuration["HostEndpoint"]}/orders/outForDelivery/{order.Id}");

                await context.CallHttpAsync(HttpMethod.Get, uri);

                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddHours(1);
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var acknowledgeTask = context.WaitForExternalEvent(Constants.RESTAURANT_ORDER_OUTFORDELIVERY_EVENT);
                    var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);

                    if (winner == acknowledgeTask)
                    {
                        string instanceId = $"{order.Id}-out-for-delivery";

                        context.StartNewOrchestration("OrderOutForDeliveryOrchestrator", order, instanceId);

                        cts.Cancel();
                    }
                    else
                    {
                        //Handle time out logic

                        log.LogError($"OrderAcceptedOrchestrator Timed out {order.Id}");

                        await context.CallActivityAsync("NotifyCustomer", order);

                        await context.CallActivityAsync("NotifyRestaurant", order);
                    }
                }
            }
            catch (Exception ex)
            {
                if (order != null)
                {
                    ex.LogExceptionDetails(log, order.Id, GetType().FullName);
                }
                else
                {
                    ex.LogExceptionDetails(log, null, GetType().FullName);
                }
            }           
        }

        [FunctionName("OrderOutForDeliveryOrchestrator")]
        public async Task OrderOutForDeliveryOrchestrator(
     [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log = context.CreateReplaySafeLogger(log); //to prevent logging at the time of function replay
            Order order = context.GetInput<Order>();
            try
            {
                order.OrderStatus = OrderStatus.OutForDelivery;

                await context.CallActivityAsync("UpsertOrder", order);

                await context.CallActivityAsync("NotifyCustomer", order);

                Uri uri = new Uri($"{_configuration["HostEndpoint"]}/orders/delivered/{order.Id}");

                await context.CallHttpAsync(HttpMethod.Get, uri);

                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddHours(1);
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var acknowledgeTask = context.WaitForExternalEvent(Constants.DELIVERY_ORDER_DELIVERED_EVENT);
                    var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);

                    if (winner == acknowledgeTask)
                    {
                        order.OrderStatus = OrderStatus.Delivered;

                        await context.CallActivityAsync("UpsertOrder", order);

                        await context.CallActivityAsync("NotifyCustomer", order);

                        cts.Cancel();
                    }
                    else
                    {
                        //Handle time out logic
                        log.LogError($"OrderOutForDeliveryOrchestrator Timed out {order.Id}");

                        await context.CallActivityAsync("NotifyCustomer", order);

                        await context.CallActivityAsync("NotifyRestaurant", order);                       
                    }
                }
            }
            catch (Exception ex)
            {
                if (order != null)
                {
                    ex.LogExceptionDetails(log, order.Id, GetType().FullName);
                }
                else
                {
                    ex.LogExceptionDetails(log, null, GetType().FullName);
                }
            }
        }

        #endregion

        #region Activity functions

        [FunctionName("UpsertOrder")]
        public static void UpsertOrder([ActivityTrigger] Order order,
       [CosmosDB(
                databaseName: "%CosmosDbFoodDeliveryDatabaseName%",
                collectionName: "%CosmosDbOrderContainerName%",
                ConnectionStringSetting = "CosmosDbConnectionString")] out dynamic document,
       ILogger log)
        {
            document = order;
            //TODO: Send notification to customer

        }

        [FunctionName("NotifyRestaurant")]
        public static void NotifyRestaurant([ActivityTrigger] Order order,
            ILogger log)
        {

            //TODO: Send notification to restaurant

        }

        [FunctionName("NotifyCustomer")]
        public static void NotifyCustomer([ActivityTrigger] Order order,
              ILogger log)
        {
            //TODO: Send notification to customer
        }
        #endregion       
    }
}
