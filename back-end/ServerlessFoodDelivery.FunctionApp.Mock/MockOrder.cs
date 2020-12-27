using System;
using System.Collections.Generic;
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

        [FunctionName("Function1")]
        public async static Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo timerInfo, ILogger log)
        {
            List<Order> orders = new List<Order>();
            for(int i = 0; i < 50; i++)
            {
                RestaurantDetails restaurantDetails = new RestaurantDetails()
                {
                    Id = "12345",
                    RestaurantName = "Rohan's Cafe",
                };

                List<MenuItem> items = new List<MenuItem>() {
                new MenuItem()
                {
                    Id = "1",
                    Item = "Pasta",
                    Price = 120,
                    DishType = DishType.Veg
                },
                new MenuItem()
                {
                    Id = "2",
                    Item = "Pizza",
                    Price = 150,
                    DishType = DishType.NonVeg
                }
            };

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

                Order order = new Order();
                order.Id = Guid.NewGuid().ToString();                
                order.RestaurantDetails = restaurantDetails;
                order.OrderItems = items;
                order.OrderStatus = Models.Enums.OrderStatus.New;
                order.Customer = customer;

                orders.Add(order);
               
            }
           
            Parallel.ForEach(orders, x => MockOrders(x, log));           

        }
        [FunctionName("FunctionHttp")]
        public async static Task FunctionHttp([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/mock")] HttpRequest req, ILogger log)
        {
            List<Order> orders = new List<Order>();
            for (int i = 0; i < 50; i++)
            {
                RestaurantDetails restaurantDetails = new RestaurantDetails()
                {
                    Id = "12345",
                    RestaurantName = "Rohan's Cafe",
                };

                List<MenuItem> items = new List<MenuItem>() {
                new MenuItem()
                {
                    Id = "1",
                    Item = "Pasta",
                    Price = 120,
                    DishType = DishType.Veg
                },
                new MenuItem()
                {
                    Id = "2",
                    Item = "Pizza",
                    Price = 150,
                    DishType = DishType.NonVeg
                }
            };

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

                Order order = new Order();
                order.Id = Guid.NewGuid().ToString();
                order.RestaurantDetails = restaurantDetails;
                order.OrderItems = items;
                order.OrderStatus = Models.Enums.OrderStatus.New;
                order.Customer = customer;

                orders.Add(order);

            }

            Parallel.ForEach(orders, x => MockOrders(x, log));

        }

        private async static void MockOrders(Order order, ILogger log)
        {
            var serializedObject = JsonConvert.SerializeObject(order);
            var stringContent = new StringContent(serializedObject, UnicodeEncoding.UTF8, "application/json");
            log.LogInformation("Placing order... " + order.Id);

            var responseOrderPlaced = await client.PostAsync("<new-order-function-url>", stringContent);
            log.LogInformation(responseOrderPlaced.StatusCode.ToString());

            if (responseOrderPlaced.IsSuccessStatusCode)
            {
                Thread.Sleep(5000);
                log.LogInformation("Accepting order... " + order.Id);
                var responseOrderAccepted = await client.GetAsync($"<accept-order-function-url>");
                log.LogInformation(responseOrderAccepted.StatusCode.ToString());

                if (responseOrderAccepted.IsSuccessStatusCode)
                {
                    Thread.Sleep(10000);
                    log.LogInformation("Out for delivery order... " + order.Id);
                    var responseOrderOutForDelivery = await client.GetAsync($"<out-for-delivery-function-url>");

                    if (responseOrderOutForDelivery.IsSuccessStatusCode)
                    {
                        Thread.Sleep(10000);
                        log.LogInformation("Delivering order... " + order.Id);
                        var responseOrderDelivered = await client.GetAsync($"<delivered-function-url>");
                        log.LogInformation(responseOrderDelivered.StatusCode.ToString());

                        if (!responseOrderDelivered.IsSuccessStatusCode)
                        {

                            log.LogError("Something went wrong while delivering the order...");
                        }

                    }
                    else
                    {
                        log.LogError("Something went wrong while marking the order as out for delivery...");
                    }
                }
                else
                {
                    log.LogError("Something went wrong while accepting the order...");
                }
            }
            else
            {
                log.LogError("Something went wrong while placing the order...");
            }

        }
    }
}
