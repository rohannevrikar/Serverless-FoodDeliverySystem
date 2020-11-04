using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Shared.Services
{
    public class SettingService : ISettingService
    {
        //Cosmos
        private const string CosmosDbEndpointUriKey = "CosmosDbEndpointUri";
        private const string CosmosDbApiKey = "CosmosDbApiKey";
        private const string CosmosDbConnectionStringKey = "CosmosDbConnectionString";
        private const string CosmosDbFoodDeliveryDatabaseNameKey = "CosmosDbFoodDeliveryDatabaseName";
        private const string CosmosDbOrderContainerNameKey = "CosmosDbOrderContainerName";
        private const string CosmosDbRestaurantContainerNameKey = "CosmosDbRestaurantContainerName";
        private const string CosmosDbCustomerContainerNameKey = "CosmosDbCustomerContainerName";

        //Storage 

        private const string StorageAccountKey = "AzureWebJobsStorage";
        private const string OrderNewQueueKey = "OrderNewQueue";
        private const string OrderAcceptedQueueKey = "OrderAcceptedQueue";
        private const string OrderOutForDeliveryQueueKey = "OrderOutForDeliveryQueue";
        private const string OrderDeliveredQueueKey = "OrderDeliveredQueue";
        private const string OrderCanceledQueueKey = "OrderCanceledQueue";





        public string GetCosmosDbApiKey()
        {
            return GetEnvironmentVariable(CosmosDbApiKey);

        }

        public string GetCosmosDbConnectionString()
        {
            return GetEnvironmentVariable(CosmosDbConnectionStringKey);
        }

        public string GetCosmosDbCustomerContainerName()
        {
            return GetEnvironmentVariable(CosmosDbCustomerContainerNameKey);
        }
        public string GetCosmosDbOrderContainerName()
        {
            return GetEnvironmentVariable(CosmosDbOrderContainerNameKey);
        }
        public string GetCosmosDbRestaurantContainerName()
        {
            return GetEnvironmentVariable(CosmosDbRestaurantContainerNameKey);
        }
        public string GetCosmosDbEndpointUri()
        {
            return GetEnvironmentVariable(CosmosDbEndpointUriKey);
        }

        public string GetCosmosDbFoodDeliveryDatabaseName()
        {
            return GetEnvironmentVariable(CosmosDbFoodDeliveryDatabaseNameKey);
        }
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public string GetStorageAccount()
        {
            return GetEnvironmentVariable(StorageAccountKey);
        }

        public string GetOrderAcceptedQueueName()
        {
            return GetEnvironmentVariable(OrderAcceptedQueueKey);
        }

        public string GetOrderOutForDeliveryQueueName()
        {
            return GetEnvironmentVariable(OrderOutForDeliveryQueueKey);
        }

        public string GetOrderDeliveredQueueName()
        {
            return GetEnvironmentVariable(OrderDeliveredQueueKey);
        }

        public string GetOrderNewQueueName()
        {
            return GetEnvironmentVariable(OrderNewQueueKey);
        }

        public string GetOrderCanceledQueueName()
        {
            return GetEnvironmentVariable(OrderCanceledQueueKey);
        }
    }
}
