using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Shared.Services
{
    public interface ISettingService
    {
        //Cosmos
        string GetCosmosDbEndpointUri();
        string GetCosmosDbApiKey();
        string GetCosmosDbConnectionString();
        string GetCosmosDbFoodDeliveryDatabaseName();
        string GetCosmosDbCustomerContainerName();
        string GetCosmosDbOrderContainerName();
        string GetCosmosDbRestaurantContainerName();

        //Storage Account

        string GetStorageAccount();
        string GetOrderNewQueueName();
        string GetOrderAcceptedQueueName();
        string GetOrderOutForDeliveryQueueName();
        string GetOrderDeliveredQueueName();
        string GetOrderCanceledQueueName();

    }
}
