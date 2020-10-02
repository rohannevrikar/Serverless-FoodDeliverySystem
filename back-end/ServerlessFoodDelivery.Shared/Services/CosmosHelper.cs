using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Shared.Services
{
    public  class CosmosHelper : ICosmosHelper
    {
        private  CosmosClient _cosmosDbSingletonClient;
        private readonly ISettingService _settingService;
        private readonly string _cosmosDbEndpointUri;
        private readonly string _cosmosDbApiKey;
        private readonly string _cosmosDbFoodDeliveryDatabaseName;
        private Database _cosmosDbSingletonDatabase;

        public CosmosHelper(ISettingService setting)
        {
            _settingService = setting;
            _cosmosDbEndpointUri = _settingService.GetCosmosDbEndpointUri();
            _cosmosDbApiKey = _settingService.GetCosmosDbApiKey();
            _cosmosDbFoodDeliveryDatabaseName = _settingService.GetCosmosDbFoodDeliveryDatabaseName();

            if (_cosmosDbSingletonClient == null)
                _cosmosDbSingletonClient = GetCosmosClient();         

        }
        private CosmosClient GetCosmosClient()
        {                       
            
            return new CosmosClient(_cosmosDbEndpointUri, _cosmosDbApiKey);
        }

        public Database GetCosmosDatabaseInstance()
        {
            if(_cosmosDbSingletonDatabase == null)
                _cosmosDbSingletonDatabase = _cosmosDbSingletonClient.CreateDatabaseIfNotExistsAsync(_cosmosDbFoodDeliveryDatabaseName).GetAwaiter().GetResult().Database;
            return _cosmosDbSingletonDatabase;
        }

    }
    public interface ICosmosHelper
    {
        Database GetCosmosDatabaseInstance();
    }
}
