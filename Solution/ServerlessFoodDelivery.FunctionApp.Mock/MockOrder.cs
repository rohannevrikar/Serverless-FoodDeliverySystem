using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFoodDelivery.Models.Models;

namespace ServerlessFoodDelivery.FunctionApp.Mock
{
    public class MockOrder : IFunctionExceptionFilter
    {
        private static readonly HttpClient client = new HttpClient();
        static Random random = new Random();

        private readonly IConfiguration _configuration;
        public MockOrder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("RunsEveryMinute")]
        public async Task RunsEveryMinute([TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo, ILogger log)
        {
          
            PrepareAndPlaceOrder(log, 60); 
        }

        public async void PrepareAndPlaceOrder(ILogger log, int numberOfOrdersToBePlaced)
        {

       
            for (int i = 0; i < numberOfOrdersToBePlaced; i++)
            {
                List<MenuItem> orderItems = new List<MenuItem>();
                var responseGetRestaurants = await client.GetAsync($"{_configuration["RestaurantHostEndpoint"]}/restaurants/getRestaurants"); //TODO: Get host address from config
                if (responseGetRestaurants.IsSuccessStatusCode)
                {
                    
                    List<Restaurant> restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(await responseGetRestaurants.Content.ReadAsStringAsync());
                    int getRandomItemIndex = random.Next(restaurants.Count);
                    var responseGetRestaurant = await client.GetAsync($"{_configuration["RestaurantHostEndpoint"]}/restaurants/getRestaurant/{restaurants[getRandomItemIndex].Id}"); //TODO: Get host address from config
                    if (responseGetRestaurant.IsSuccessStatusCode)
                    {                        
                        Restaurant restaurant = JsonConvert.DeserializeObject<Restaurant>(await responseGetRestaurant.Content.ReadAsStringAsync());

                        orderItems = restaurant.MenuItems.OrderBy(x => Guid.NewGuid()).Take(3).ToList(); //to randomly select 3 menu items
                        Customer customer = new Customer()
                        {
                            Id = "1",
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "joh.doe@gmail.com",
                            ContactNumber = "1234567",
                            Address = "22 Jump street",
                            IsBlocked = false
                        };
                        Order order = new Order()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Customer = customer,
                            RestaurantId = restaurant.Id,
                            RestaurantName = restaurant.RestaurantName,
                            OrderItems = orderItems,
                            OrderStatus = Models.Enums.OrderStatus.New
                        };
                        MockOrders(order, log);
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        throw new Exception("Failed to get restaurant");
                    }
                }
                else
                {
                    throw new Exception("Failed to get list of restaurants");
                }
            }
           
        }

        private async void MockOrders(Order order, ILogger log)
        {
            var serializedObject = JsonConvert.SerializeObject(order);
            var stringContent = new StringContent(serializedObject, UnicodeEncoding.UTF8, "application/json");
            var responsePlaceOrder = await client.PostAsync($"{_configuration["OrderHostEndpoint"]}/orders", stringContent);
            if (!responsePlaceOrder.IsSuccessStatusCode)
            {
                throw new Exception("Failed to place a new order");

            }
        }

        public Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            exceptionContext.Logger.LogError($"Something went wrong while executing {exceptionContext.FunctionName}. Exception details: {exceptionContext.Exception}");
            return Task.CompletedTask;
        }
    }
}
