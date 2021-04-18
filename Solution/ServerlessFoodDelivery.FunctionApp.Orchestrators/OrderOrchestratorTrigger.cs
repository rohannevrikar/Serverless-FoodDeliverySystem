using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ServerlessFoodDelivery.Models.Models;
using ServerlessFoodDelivery.Shared;

namespace ServerlessFoodDelivery.FunctionApp.Orchestrators
{
    public class OrderOrchestratorTrigger
    {
        #region QueueTriggers

        [FunctionName("PlaceNewOrderQueueTrigger")]
        public async Task PlaceNewOrderQueueTrigger(
        [DurableClient] IDurableOrchestrationClient context,
        [ServiceBusTrigger("%OrderNewQueue%", Connection = "ServiceBusConnection")] Order order,
        ILogger log)
        {
            try
            {
                string instanceId = order.Id;
                await StartInstance(context, order, instanceId, log);
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

        [FunctionName("AcceptedOrderQueueTrigger")]
        public async Task AcceptedOrderQueueTrigger(
        [DurableClient] IDurableOrchestrationClient context,
        [ServiceBusTrigger("%OrderAcceptedQueue%", Connection = "ServiceBusConnection")] Order order,
        ILogger log)
        {
            try
            {
                string instanceId = order.Id;
                await context.RaiseEventAsync(instanceId, Constants.RESTAURANT_ORDER_ACCEPT_EVENT);
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

        [FunctionName("OutForDeliveryOrderQueueTrigger")]
        public async Task OutForDeliveryOrderQueueTrigger(
        [DurableClient] IDurableOrchestrationClient context,
        [ServiceBusTrigger("%OrderOutForDeliveryQueue%", Connection = "ServiceBusConnection")] Order order,
        ILogger log)
        {
            try
            {
                string instanceId = $"{order.Id}-accepted";
                await context.RaiseEventAsync(instanceId, Constants.RESTAURANT_ORDER_OUTFORDELIVERY_EVENT);
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

        [FunctionName("DeliveredOrderQueueTrigger")]
        public async Task DeliveredOrderQueueTrigger(
        [DurableClient] IDurableOrchestrationClient context,
        [ServiceBusTrigger("%OrderDeliveredQueue%", Connection = "ServiceBusConnection")] Order order,
        ILogger log)
        {
            try
            {
                string instanceId = $"{order.Id}-out-for-delivery";
                await context.RaiseEventAsync(instanceId, Constants.DELIVERY_ORDER_DELIVERED_EVENT);
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

        private async Task StartInstance(IDurableOrchestrationClient context, Order order, string instanceId, ILogger log)
        {
            try
            {
                var reportStatus = await context.GetStatusAsync(instanceId);
                string runningStatus = reportStatus == null ? "NULL" : reportStatus.RuntimeStatus.ToString();
                //log.LogInformation($"Instance running status: '{runningStatus}'.");

                if (reportStatus == null || reportStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
                {
                    await context.StartNewAsync("OrderPlacedOrchestrator", instanceId, order);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #region Handling DLQs
        [FunctionName("AcceptedOrderDeadLetterQueueTrigger")]
        public async Task AcceptedOrderDeadLetterQueueTrigger(
 [DurableClient] IDurableOrchestrationClient context,
 [ServiceBusTrigger("%OrderAcceptedQueue%/$DeadLetterQueue", Connection = "ServiceBusConnection")] Order order,
 ILogger log)
        {
            log.LogInformation("clearing dead letter queue");
        }

        [FunctionName("OutForDeliveryOrderDeadLetterQueueTrigger")]
        public async Task OutForDeliveryOrderDeadLetterQueueTrigger(
     [DurableClient] IDurableOrchestrationClient context,
     [ServiceBusTrigger("%OrderOutForDeliveryQueue%/$DeadLetterQueue", Connection = "ServiceBusConnection")] Order order,
     ILogger log)
        {
            log.LogInformation("clearing dead letter queue");
        }

        [FunctionName("DeliveredOrderDeadLetterQueueTrigger")]
        public async Task DeliveredOrderDeadLetterQueueTrigger(
     [DurableClient] IDurableOrchestrationClient context,
     [ServiceBusTrigger("%OrderDeliveredQueue%/$DeadLetterQueue", Connection = "ServiceBusConnection")] Order order,
     ILogger log)
        {
            log.LogInformation("clearing dead letter queue");
        }
        #endregion        
    }
}