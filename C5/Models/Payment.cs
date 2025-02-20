using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C5.Models
{
    public class Payment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string OrderId { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán không được để trống.")]
        [StringLength(50, ErrorMessage = "Phương thức thanh toán không được vượt quá 50 ký tự.")]
        public string PaymentMethod { get; set; } // VNPay, Momo, Tiền mặt

        [Required]
        [Range(0, 1, ErrorMessage = "Trạng thái thanh toán không hợp lệ.")]
        public int Status { get; set; } = 0; // 0: Chờ thanh toán, 1: Đã thanh toán

        [Required]
        public DateTime PaymentDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; }
    }
}
