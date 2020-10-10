using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessFoodDelivery.Models
{
    public class Enums
    {
        public enum ItemTypes
        {
           Order,
           Customer,
           Restaurant
        }
        public enum OrderStatus
        {
            Unassigned,
            New,
            Accepted,
            OutForDelivery,
            Delivered,
            DeliveryFailed,
            Canceled 
        }
    }
}
