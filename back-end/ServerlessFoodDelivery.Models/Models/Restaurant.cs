using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Models.Models
{
    public class Restaurant
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "restaurantName")]
        public string RestaurantName { get; set; }

        [JsonProperty(PropertyName = "menu")]
        public List<MenuItem> MenuItems{ get; set; }

        [JsonProperty(PropertyName = "isAcceptingOrders")]
        public bool IsAcceptingOrders { get; set; }
    }
   

    public class RestaurantDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "restaurantName")]
        public string RestaurantName { get; set; }

    }
}
