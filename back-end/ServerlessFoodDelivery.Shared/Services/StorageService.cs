using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using ServerlessFoodDelivery.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFoodDelivery.Shared.Services
{
    public class StorageService : IStorageService
    {
        private bool _isStorageInitialized = false;
        private ISettingService _settingService;
        private CloudQueue _orderNewQueue;
        private CloudQueue _orderAcceptedQueue;
        private CloudQueue _orderOutForDeliveryQueue;
        private CloudQueue _orderDeliveredQueue;
        private CloudQueue _orderCanceledQueue;


        public StorageService(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task EnqueueOrderForStatusUpdate(string orderId, OrderStatus orderStatus)
        {
            await InitializeStorage();

            var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(new OrderLite
            {
                Id = orderId,
                OrderStatus = orderStatus
            }));


            if (orderStatus == OrderStatus.New)
                await _orderNewQueue.AddMessageAsync(queueMessage);

            else if (orderStatus == OrderStatus.InProgress)
                await _orderAcceptedQueue.AddMessageAsync(queueMessage);

            else if (orderStatus == OrderStatus.OutForDelivery)
                await _orderOutForDeliveryQueue.AddMessageAsync(queueMessage);

            else if (orderStatus == OrderStatus.Delivered)
                await _orderDeliveredQueue.AddMessageAsync(queueMessage);  

            else if(orderStatus == OrderStatus.Canceled)
                await _orderCanceledQueue.AddMessageAsync(queueMessage);
        }


        private async Task InitializeStorage()
        {

            if (_isStorageInitialized)
                return;

            if (!string.IsNullOrEmpty(_settingService.GetStorageAccount()))
            {
                var queueStorageAccount = CloudStorageAccount.Parse(_settingService.GetStorageAccount());
                var queueClient = queueStorageAccount.CreateCloudQueueClient();

                var orderNewQueueName = _settingService.GetOrderNewQueueName();
                if (!string.IsNullOrEmpty(orderNewQueueName))
                {
                    _orderNewQueue = queueClient.GetQueueReference(orderNewQueueName);
                    await _orderNewQueue.CreateIfNotExistsAsync();
                }

                var orderAcceptedQueueName = _settingService.GetOrderAcceptedQueueName();
                if (!string.IsNullOrEmpty(orderAcceptedQueueName))
                {
                    _orderAcceptedQueue = queueClient.GetQueueReference(orderAcceptedQueueName);
                    await _orderAcceptedQueue.CreateIfNotExistsAsync();
                }

                var orderOutForDeliveryQueueName = _settingService.GetOrderOutForDeliveryQueueName();
                if (!string.IsNullOrEmpty(orderOutForDeliveryQueueName))
                {
                    _orderOutForDeliveryQueue = queueClient.GetQueueReference(orderOutForDeliveryQueueName);
                    await _orderOutForDeliveryQueue.CreateIfNotExistsAsync();
                }

                var orderDeliveredQueueName = _settingService.GetOrderDeliveredQueueName();
                if (!string.IsNullOrEmpty(orderDeliveredQueueName))
                {
                    _orderDeliveredQueue = queueClient.GetQueueReference(orderDeliveredQueueName);
                    await _orderDeliveredQueue.CreateIfNotExistsAsync();
                }

                var orderCanceledQueueName = _settingService.GetOrderCanceledQueueName();
                if (!string.IsNullOrEmpty(orderCanceledQueueName))
                {
                    _orderCanceledQueue = queueClient.GetQueueReference(orderCanceledQueueName);
                    await _orderCanceledQueue.CreateIfNotExistsAsync();
                }

                _isStorageInitialized = true;

            }
        }
    }

    public interface IStorageService
    {
        Task EnqueueOrderForStatusUpdate(string orderId, OrderStatus orderStatus);
    }
}

