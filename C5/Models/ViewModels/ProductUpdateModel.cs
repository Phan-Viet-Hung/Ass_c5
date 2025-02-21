namespace C5.Models.ViewModels
{
    public class ProductUpdateModel
    {
        public string Id { get; set; }  // ID sản phẩm cần cập nhật
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? ImageFile { get; set; }  // Ảnh mới (nếu có)
    }

}
