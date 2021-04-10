using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatbot.Models
{
    public class Order
    {
        public bool isActive { get; set; } = false;

        public String DeliveryMethod { get; set; }

        public List<Product> OrderedProducts { get; set; } = new List<Product>();
    }
}