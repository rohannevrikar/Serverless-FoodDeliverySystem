using Microsoft.Azure.Cosmos;
using ServerlessFoodDelivery.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFoodDelivery.Shared.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly Container _cosmosDbSingletonContainer;
        private readonly string _cosmosDbContainerName;
        private readonly ISettingService _settingService;

        public CustomerService(ISettingService settingSerivce, ICosmosHelper cosmosHelper)
        {
            _settingService = settingSerivce;
            _cosmosDbContainerName = _settingService.GetCosmosDbCustomerContainerName();

            if (_cosmosDbSingletonContainer == null)
                _cosmosDbSingletonContainer = cosmosHelper.GetCosmosDatabaseInstance().CreateContainerIfNotExistsAsync(_cosmosDbContainerName, "/id").GetAwaiter().GetResult();
        }

        public async Task<Customer> AddCustomer(Customer customer)
        {
            return await _cosmosDbSingletonContainer.CreateItemAsync(customer);            
        }

        public async Task<Customer> BlockCustomer(string customerId)
        {
            Customer customer = await _cosmosDbSingletonContainer.ReadItemAsync<Customer>(customerId, new PartitionKey(customerId));
            customer.IsBlocked = true;
            return await _cosmosDbSingletonContainer.ReplaceItemAsync(customer, customerId, new PartitionKey(customerId));
        }

        public async Task<Customer> GetCustomer(string customerId)
        {
            return await _cosmosDbSingletonContainer.ReadItemAsync<Customer>(customerId, new PartitionKey(customerId));
        }

        public async Task<List<Customer>> GetCustomers()
        {
            var sqlQueryText = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Customer> queryResultSetIterator = _cosmosDbSingletonContainer.GetItemQueryIterator<Customer>(queryDefinition);
            List<Customer> Customers = new List<Customer>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Customer> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Customer customer in currentResultSet)
                {
                    Customers.Add(customer);
                }
            }
            return Customers;
           
        }
      
    }
    public interface ICustomerService
    {
        Task<Customer> AddCustomer(Customer customer);
        Task<Customer> BlockCustomer(string customerId);
        Task<Customer> GetCustomer(string customerId);
        Task<List<Customer>> GetCustomers();

    }
}
