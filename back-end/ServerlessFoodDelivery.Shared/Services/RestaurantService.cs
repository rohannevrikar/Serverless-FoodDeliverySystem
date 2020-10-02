using Microsoft.Azure.Cosmos;
using ServerlessFoodDelivery.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFoodDelivery.Shared.Services
{
    public class RestaurantService : IRestaurantService
    {

        private readonly Container _cosmosDbSingletonContainer;
        private readonly string _cosmosDbContainerName;
        private readonly ISettingService _settingService;

        public RestaurantService(ISettingService settingSerivce, ICosmosHelper cosmosHelper)
        {
            _settingService = settingSerivce;
            _cosmosDbContainerName = _settingService.GetCosmosDbRestaurantContainerName();

            if (_cosmosDbSingletonContainer == null)
                _cosmosDbSingletonContainer = cosmosHelper.GetCosmosDatabaseInstance().CreateContainerIfNotExistsAsync(_cosmosDbContainerName, "/id").GetAwaiter().GetResult();

        }

        public async Task<Restaurant> GetRestaurant(string restaurantId)
        {
            return await _cosmosDbSingletonContainer.ReadItemAsync<Restaurant>(restaurantId, new PartitionKey(restaurantId));
        }

        public async Task<List<Restaurant>> GetRestaurants()
        {
            var sqlQueryText = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Restaurant> queryResultSetIterator = _cosmosDbSingletonContainer.GetItemQueryIterator<Restaurant>(queryDefinition);
            List<Restaurant> Restaurants = new List<Restaurant>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Restaurant> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Restaurant restaurant in currentResultSet)
                {
                    Restaurants.Add(restaurant);
                }
            }
            return Restaurants;
        }
    }
    public interface IRestaurantService
    {
        Task<Restaurant> GetRestaurant(string restaurantId);
        Task<List<Restaurant>> GetRestaurants();
    }
}
