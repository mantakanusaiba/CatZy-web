namespace Catzy.Models
{
    public class OrderListItem
    {
        public int OrderId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string FullName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
    }
}