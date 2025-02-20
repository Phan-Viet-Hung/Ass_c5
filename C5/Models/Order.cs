using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C5.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }
        public Guid? VoucherId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0.")]
        public decimal TotalAmount { get; set; }

        public enum OrderStatus
        {
            Pending = 0,
            Delivering = 1,
            Completed = 2,
            Canceled = 3,
            WaitingForPayment = 4
        }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Required]
        public string PaymentMethod { get; set; }
        [Required]
        public DateTime OrderDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        [ForeignKey(nameof(UserId))]
        public virtual FastFoodUser User { get; set; }

        [ForeignKey(nameof(VoucherId))]
        public virtual Voucher? Voucher { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Notification>? Notifications { get; set; }

        public virtual Payment Payment { get; set; }
        public decimal? DiscountPercent { get; internal set; }
        public decimal? DiscountAmount { get; internal set; }
        public decimal? FinalAmount { get; internal set; }
        public string? VoucherCode { get; internal set; }
        public int OrderInformation { get; internal set; }
    }
}
