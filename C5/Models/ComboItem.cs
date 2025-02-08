using C5.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ComboItem
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string ComboId { get; set; }

    [Required]
    public string ProductId { get; set; }

    [Required(ErrorMessage = "Số lượng sản phẩm trong combo không được để trống.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public int Quantity { get; set; }

    [ForeignKey("ComboId")]
    public virtual Combo Combo { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }
}
