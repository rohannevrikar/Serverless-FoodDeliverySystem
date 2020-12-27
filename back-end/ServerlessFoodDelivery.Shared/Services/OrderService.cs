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
        private readonly IStorageService _storageService;


        public OrderService(ISettingService settingSerivce, ICosmosHelper cosmosHelper, IStorageService storageService)
        {
             _settingService = settingSerivce;
             _cosmosDbContainerName = _settingService.GetCosmosDbOrderContainerName();
            _storageService = storageService;

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
            try
            {
                Order placedOrder = await _cosmosDbSingletonContainer.CreateItemAsync(order);
                return placedOrder;
            }
            catch(Exception ex)
            {
                throw ex;
            }
         
        }

        public async Task<Order> UpdateOrder(Order order)
        {           
            Order updatedOrder = await _cosmosDbSingletonContainer.ReplaceItemAsync(order, order.Id);            
            return updatedOrder;
        }     
    }

    public interface IOrderService
    {

        Task<Order> GetOrder(string orderId);
        Task<List<Order>> GetOrders();
        Task<Order> UpdateOrder(Order order);
        Task<Order> PlaceNewOrder(Order order);
    }
}
