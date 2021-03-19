using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Models.Models
{
    public class Order
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "restaurantId")]
        public string RestaurantId { get; set; }

        [JsonProperty(PropertyName = "restaurantName")]
        public string RestaurantName { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "orderStatus")]
        public Enums.OrderStatus OrderStatus { get; set; }

        [JsonProperty(PropertyName = "orderItems")]
        public List<MenuItem> OrderItems { get; set; }
    }
    

    public class OrderLite
    {
        public string Id { get; set; }
        public Enums.OrderStatus OrderStatus { get; set; }
    }
 
}
