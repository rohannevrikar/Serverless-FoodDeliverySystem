using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ServerlessFoodDelivery.Models.Models;

namespace ServerlessFoodDelivery.FunctionApp.Restaurants
{
    public class RestaurantFunction
    {       

        [FunctionName("GetRestaurants")]
        public IActionResult GetRestaurants(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "restaurants/getRestaurants")] HttpRequest req,
             [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Restaurants",
                ConnectionStringSetting = "CosmosDbConnectionString",
                SqlQuery = "SELECT * FROM c")]
                IEnumerable<Restaurant> restaurants,
            ILogger log)
        {
           
            return new OkObjectResult(restaurants);
        }

        [FunctionName("GetRestaurant")]
        public IActionResult GetRestaurant(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "restaurants/getRestaurant/{restaurantId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "FoodDeliveryDB",
                collectionName: "Restaurants",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{restaurantId}",
                PartitionKey = "{restaurantId}")] Restaurant restaurant,
           ILogger log)
        {
            return new OkObjectResult(restaurant);
        }
    }
}
