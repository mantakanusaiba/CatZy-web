using System;
using System.Collections.Generic;

namespace Catzy.Models
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public List<CartItem> Items { get; set; }
    }
}