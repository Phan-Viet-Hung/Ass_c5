namespace C5.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
        public decimal DiscountPercent { get; set; } = 0; // Phần trăm giảm giá (0-100)
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public string? VoucherCode { get; set; }
    }
}
