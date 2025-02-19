namespace C5.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual FastFoodUser? User { get; set; }
        public string? OrderId { get; set; }  // Có thể null nếu thông báo không liên quan đến đơn hàng
        public virtual Order? Order { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        public bool IsRead { get; set; } = false;
    }

}
