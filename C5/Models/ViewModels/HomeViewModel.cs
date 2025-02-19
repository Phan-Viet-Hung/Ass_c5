using X.PagedList;

namespace C5.Models.ViewModels
{
    public class HomeViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public IPagedList<Combo> Combos { get; set; }
    }
}
