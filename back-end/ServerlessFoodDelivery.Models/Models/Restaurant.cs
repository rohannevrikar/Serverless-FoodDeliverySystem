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

        [JsonProperty(PropertyName = "menu")]
        public Menu Menu{ get; set; }

        [JsonProperty(PropertyName = "isAcceptingOrders")]
        public bool IsAcceptingOrders { get; set; }
    }

    public class Menu
    {
        [JsonProperty(PropertyName = "id")]
        public string Id{ get; set; }

        [JsonProperty(PropertyName = "item")]
        public string Item { get; set; }

        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "dishType")]
        public DishType DishType{ get; set; }
    }
    public enum DishType
    {
        Veg = 0,
        NonVeg = 1,
        ContainsEgg = 2
    }
}
