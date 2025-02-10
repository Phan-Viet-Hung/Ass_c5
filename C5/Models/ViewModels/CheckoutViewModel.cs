namespace C5.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
