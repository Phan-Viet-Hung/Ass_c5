using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C5.Models
{
    public class CartItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CartId { get; set; }
        public string? ProductId { get; set; }
        public string? ComboId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
        public Combo Combo { get; set; }
        [ForeignKey(nameof(CartId))]
        public virtual Cart Cart { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }
}
