using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace C5.Models
{
    public class Voucher
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Mã giảm giá không được để trống.")]
        [StringLength(20, ErrorMessage = "Mã giảm giá không được vượt quá 20 ký tự.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm giá.")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải nằm trong khoảng từ 0 đến 100.")]
        public decimal DiscountPercent { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Required]
        [Range(0,1000,ErrorMessage = "Số lượng chỉ được từ 0 đến 1000")]
        public int Quantity { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
