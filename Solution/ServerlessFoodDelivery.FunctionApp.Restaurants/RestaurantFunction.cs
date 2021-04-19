using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ServerlessFoodDelivery.Models.Models;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using System.Threading;

namespace ServerlessFoodDelivery.FunctionApp.Restaurants
{
    public class RestaurantFunction : IFunctionExceptionFilter
    {       

        [FunctionName("GetRestaurants")]
        public IActionResult GetRestaurants(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "restaurants/getRestaurants")] HttpRequest req,
             [CosmosDB(
                databaseName: "%CosmosDbFoodDeliveryDatabaseName%",
                collectionName: "%CosmosDbRestaurantContainerName%",
                ConnectionStringSetting = "CosmosDbConnectionString",
                SqlQuery = "SELECT * FROM c")]
                IEnumerable<Restaurant> restaurants)
        {
           
            return new OkObjectResult(restaurants);
        }

        [FunctionName("GetRestaurant")]
        public IActionResult GetRestaurant(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "restaurants/getRestaurant/{restaurantId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%CosmosDbFoodDeliveryDatabaseName%",
                collectionName: "%CosmosDbRestaurantContainerName%",
                ConnectionStringSetting = "CosmosDbConnectionString",
                Id = "{restaurantId}",
                PartitionKey = "{restaurantId}")] Restaurant restaurant)
        {
            return new OkObjectResult(restaurant);
        }

        public Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            exceptionContext.Logger.LogError($"Something went wrong while executing {exceptionContext.FunctionName}. Exception details: {exceptionContext.Exception}");
            return Task.CompletedTask;
        }
    }
}
