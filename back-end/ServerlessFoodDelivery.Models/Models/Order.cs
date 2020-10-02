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

        [JsonProperty(PropertyName = "restaurant")]
        public Restaurant Restaurant { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "orderStatus")]
        public OrderStatus OrderStatus { get; set; }
    }
    public enum OrderStatus
    {
        New = 0,
        InProgress = 1,
        OutForDelivery = 2,
        Delivered = 3,
        DeliveryFailed = 4,
        Canceled = 5
    }
}
