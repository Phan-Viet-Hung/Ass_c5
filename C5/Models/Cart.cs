using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C5.Models
{
    public class Cart
    {
        [Key]

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual FastFoodUser User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
