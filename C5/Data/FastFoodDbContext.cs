using C5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
namespace C5.Data
{
    public class FastFoodDbContext : IdentityDbContext<IdentityUser>
    {
        public FastFoodDbContext(DbContextOptions<FastFoodDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    builder.Entity<FastFoodUser>()
        //        .HasOne(u => u.Cart)
        //        .WithOne(c => c.User)
        //        .HasForeignKey<Cart>(c => c.UserId);
        //    builder.Entity<FastFoodUser>(entity =>
        //    {
        //        entity.Property(e => e.FullName).HasMaxLength(255);
        //        entity.Property(e => e.DateOfBirth).IsRequired();
        //        entity.Property(e => e.CreatedAt).IsRequired();
        //    });
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 FastFoodUser - Sử dụng UserId làm khóa chính trong Cart
            modelBuilder.Entity<Cart>()
                .HasKey(c => c.UserId);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Quan hệ Cart - CartItem
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Quan hệ Product - Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Thiết lập quan hệ giữa User và Order (1-N)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Nếu xóa User, tất cả đơn hàng của User sẽ bị xóa

            // 🔹 Quan hệ Order - OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Quan hệ Order - Voucher
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId)
                .OnDelete(DeleteBehavior.SetNull);

            // Quan hệ Order - Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Quan hệ Review - Product & User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // Thiết lập quan hệ giữa User và Notification (1-N)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Nếu xóa User, tất cả thông báo của User sẽ bị xóa

            // Quan hệ giữa Order và Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Order)
                .WithMany(o => o.Notifications)
                .HasForeignKey(n => n.OrderId)
                .OnDelete(DeleteBehavior.Restrict);  // Ngăn xóa Order nếu có Notification liên kết
            modelBuilder.Entity<ComboItem>()
        .HasOne(ci => ci.Combo)
        .WithMany(c => c.ComboItems)
        .HasForeignKey(ci => ci.ComboId)
        .OnDelete(DeleteBehavior.Cascade); // Khi xóa Combo, tự động xóa ComboItem liên quan

            // Cấu hình quan hệ giữa Product và ComboItem
            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho phép xóa sản phẩm nếu đang thuộc Combo
        }

    }
}
