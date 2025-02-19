using C5.Models;

namespace C5.Models.ViewModels
{
    internal class OrderDetailsViewModel
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public Order.OrderStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public object OrderItems { get; set; }
    }
}