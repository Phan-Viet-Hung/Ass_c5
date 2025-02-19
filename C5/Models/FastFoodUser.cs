using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace C5.Models
{
    public class FastFoodUser : IdentityUser
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên đầy đủ phải từ 2 đến 100 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống.")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; } = GetVietnamTime();

        public string? Address { get; set; }

        public virtual Cart? Cart { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Notification>? Notifications { get; set; }


        // Lấy ngày giờ Việt Nam (GMT+7)
        private static DateTime GetVietnamTime()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        }
    }
}
