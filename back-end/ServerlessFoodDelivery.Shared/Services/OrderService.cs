using Microsoft.Azure.Cosmos;
using ServerlessFoodDelivery.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFoodDelivery.Shared.Services
{
    public class OrderService : IOrderService
    {
        private readonly Container _cosmosDbSingletonContainer;
        private readonly string _cosmosDbContainerName;
        private readonly ISettingService _settingService;


        public OrderService(ISettingService settingSerivce, ICosmosHelper cosmosHelper)
        {
             _settingService = settingSerivce;
             _cosmosDbContainerName = _settingService.GetCosmosDbOrderContainerName();

            if (_cosmosDbSingletonContainer == null)
                 _cosmosDbSingletonContainer = cosmosHelper.GetCosmosDatabaseInstance().CreateContainerIfNotExistsAsync(_cosmosDbContainerName, "/id").GetAwaiter().GetResult();

        }
        public async Task<Order> GetOrder(string orderId)
        {
            return await _cosmosDbSingletonContainer.ReadItemAsync<Order>(orderId, new PartitionKey(orderId));
        }

        public async Task<List<Order>> GetOrders()
        {
            var sqlQueryText = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Order> queryResultSetIterator = _cosmosDbSingletonContainer.GetItemQueryIterator<Order>(queryDefinition);
            List<Order> Orders = new List<Order>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Order> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Order order in currentResultSet)
                {
                    Orders.Add(order);
                }
            }
            return Orders;
        }

        public async Task<Order> PlaceNewOrder(Order order)
        {
            return await _cosmosDbSingletonContainer.CreateItemAsync(order);
        }

        public async Task<Order> UpdateOrder(string orderId, OrderStatus orderStatus)
        {
            Order order = await _cosmosDbSingletonContainer.ReadItemAsync<Order>(orderId, new PartitionKey(orderId));
            order.OrderStatus = orderStatus;
            return await _cosmosDbSingletonContainer.ReplaceItemAsync(order, orderId, new PartitionKey(orderId)); 
        }
      
    }

    public interface IOrderService
    {

        Task<Order> GetOrder(string orderId);
        Task<List<Order>> GetOrders();
        Task<Order> UpdateOrder(string orderId, OrderStatus orderStatus);
        Task<Order> PlaceNewOrder(Order order);
    }
}
