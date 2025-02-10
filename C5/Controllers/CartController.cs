using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace C5.Controllers
{
    public class CartController : Controller
    {
        private readonly FastFoodDbContext _context;
        public CartController(FastFoodDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> CartIndex()
        {
            // Lấy UserId của người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tìm giỏ hàng của user
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product) // Nạp thêm thông tin sản phẩm
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Không có sản phẩm nào trong giỏ hàng.";
                return View(new List<CartItem>());
            }

            return View(cart.CartItems.ToList());
        }
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(productId) || quantity <= 0)
            {
                return BadRequest("Sản phẩm không hợp lệ hoặc số lượng phải lớn hơn 0.");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng của user, nếu chưa có thì tạo mới
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                // Nếu có thì cập nhật số lượng
                cartItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có thì thêm mới
                cartItem = new CartItem
                {
                    CartId = cart.UserId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("CartIndex");
        }
    }
}
