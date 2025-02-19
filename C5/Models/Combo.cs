using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C5.Models
{ 
    public class Combo
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Tên combo không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên combo tối đa 100 ký tự.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả combo tối đa 500 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá combo không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá combo phải lớn hơn hoặc bằng 0.")]
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true; // Mặc định sản phẩm hoạt động
        [NotMapped]
        public string StatusText
        {
            get
            {
                return IsActive ? "Còn hàng" : "Hết hàng";
            }
        }
        public string Image { get; set; }

        public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
    }
}