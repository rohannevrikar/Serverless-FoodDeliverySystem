using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Shared.Services
{
    public class SettingService : ISettingService
    {
        private const string CosmosDbEndpointUriKey = "CosmosDbEndpointUri";
        private const string CosmosDbApiKey = "CosmosDbApiKey";
        private const string CosmosDbConnectionStringKey = "CosmosDbConnectionString";
        private const string CosmosDbFoodDeliveryDatabaseNameKey = "CosmosDbFoodDeliveryDatabaseName";
        private const string CosmosDbOrderContainerNameKey = "CosmosDbOrderContainerName";
        private const string CosmosDbRestaurantContainerNameKey = "CosmosDbRestaurantContainerName";
        private const string CosmosDbCustomerContainerNameKey = "CosmosDbCustomerContainerName";


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
    }
}
