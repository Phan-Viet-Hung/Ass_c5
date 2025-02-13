using System.ComponentModel.DataAnnotations;

namespace C5.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Danh mục sản phẩm không được để trống.")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Ảnh sản phẩm không được để trống.")]
        [Url(ErrorMessage = "Ảnh phải là một URL hợp lệ.")]
        public string Image { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho không được để trống.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không thể là số âm.")]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }

        public string StatusText { get; set; }
    }
}