using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public decimal Price { get; set; }

    [Url(ErrorMessage = "Ảnh combo phải là một URL hợp lệ.")]
    public string Image { get; set; }

    public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
}
