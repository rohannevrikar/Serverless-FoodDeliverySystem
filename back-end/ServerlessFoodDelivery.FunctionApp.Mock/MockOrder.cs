using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFoodDelivery.Models.Models;

namespace ServerlessFoodDelivery.FunctionApp.Mock
{
    public static class MockOrder
    {
        private static readonly HttpClient client = new HttpClient();
        static Stopwatch timer = new Stopwatch();
        static Random random = new Random();


        [FunctionName("RunsEveryHour")]
        public async static Task RunsEveryHour([TimerTrigger("0 */60 * * * *")] TimerInfo timerInfo, ILogger log)
        {
           //PrepareAndPlaceOrder(log, 50);
        }

        [FunctionName("RunsEveryTwoMinutes")]
        public async static Task RunsEveryMinute([TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo, ILogger log)
        {
          
            PrepareAndPlaceOrder(log, 5); 
        }

        public async static void PrepareAndPlaceOrder(ILogger log, int numberOfOrdersToBePlaced)
        {

       
            for (int i = 0; i < numberOfOrdersToBePlaced; i++)
            {
                List<MenuItem> orderItems = new List<MenuItem>();
                var responseGetRestaurants = await client.GetAsync($"<host>/api/restaurants/getRestaurants"); //TODO: Get host address from config
                if (responseGetRestaurants.IsSuccessStatusCode)
                {
                    //log.LogInformation("Got list of restaurants...");

                    List<Restaurant> restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(await responseGetRestaurants.Content.ReadAsStringAsync());
                    int getRandomItemIndex = random.Next(restaurants.Count);
                    var responseGetRestaurant = await client.GetAsync($"<host>/api/restaurants/getRestaurant/{restaurants[getRandomItemIndex].Id}"); //TODO: Get host address from config
                    if (responseGetRestaurant.IsSuccessStatusCode)
                    {
                        //log.LogInformation("Got restaurant details, preparing order...");
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
                }
            }
           
        }

        [FunctionName("FunctionHttp")]
        public async static Task FunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/mock")] HttpRequest req, ILogger log)
        {

            List<MenuItem> orderItems = new List<MenuItem>();
            var responseGetRestaurants = await client.GetAsync($"<host>/api/restaurants/getRestaurants"); //TODO: Get host address from config
            if (responseGetRestaurants.IsSuccessStatusCode)
            {
                List<Restaurant> restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(await responseGetRestaurants.Content.ReadAsStringAsync());
                int getRandomItemIndex = random.Next(restaurants.Count);
                var responseGetRestaurant = await client.GetAsync($"<host>/api/restaurants/getRestaurant/{restaurants[getRandomItemIndex].Id}"); //TODO: Get host address from config
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
                }
            }         

        }

        private async static void MockOrders(Order order, ILogger log)
        {
            var serializedObject = JsonConvert.SerializeObject(order);
            var stringContent = new StringContent(serializedObject, UnicodeEncoding.UTF8, "application/json");

            await client.PostAsync("<host>/api/orders", stringContent); //TODO: Get host address from config

           
        }
    }
}
