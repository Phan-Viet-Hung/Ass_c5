namespace C5.Models.ViewModels
{
    public class VoucherListViewModel
    {
        public List<Voucher> Vouchers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}
