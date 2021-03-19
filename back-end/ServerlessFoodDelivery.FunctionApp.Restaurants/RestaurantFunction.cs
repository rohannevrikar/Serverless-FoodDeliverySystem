using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFoodDelivery.Shared.Services;
using ServerlessFoodDelivery.Models.Models;
using System.Collections.Generic;

namespace ServerlessFoodDelivery.FunctionApp.Restaurants
{
    public class RestaurantFunction
    {
        private readonly IRestaurantService _restaurantService;
        //private readonly IStorageService _storageService;
        public RestaurantFunction(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        [FunctionName("GetRestaurants")]
        public async Task<IActionResult> GetRestaurants(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "restaurants/getRestaurants")] HttpRequest req,
            ILogger log)
        {
           
            return new OkObjectResult(await _restaurantService.GetRestaurants());
        }

        [FunctionName("GetRestaurant")]
        public async Task<IActionResult> GetRestaurant(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "restaurants/getRestaurant/{restaurantId}")] HttpRequest req,
           string restaurantId,
           ILogger log)
        {
            return new OkObjectResult(await _restaurantService.GetRestaurant(restaurantId));
        }
    }
}
