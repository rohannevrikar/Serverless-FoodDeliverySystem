using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Shared.Services
{
    public interface ISettingService
    {
        string GetCosmosDbEndpointUri();
        string GetCosmosDbApiKey();
        string GetCosmosDbConnectionString();
        string GetCosmosDbFoodDeliveryDatabaseName();
        string GetCosmosDbCustomerContainerName();
        string GetCosmosDbOrderContainerName();
        string GetCosmosDbRestaurantContainerName();


    }
}
