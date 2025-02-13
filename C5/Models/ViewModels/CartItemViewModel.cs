using System.ComponentModel.DataAnnotations;

namespace C5.Models.ViewModels
{
    public class CartItemViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm không được vượt quá 150 ký tự.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }
    }
}
