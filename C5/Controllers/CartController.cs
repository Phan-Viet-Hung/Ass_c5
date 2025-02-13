using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace C5.Controllers
{
    public class CartController : Controller
    {
        private readonly FastFoodDbContext _context;
        public CartController(FastFoodDbContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> CartIndex()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return View(new List<CartItem>());
            }

            return View(cart.CartItems.ToList());
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(string productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(productId) || quantity <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ hoặc số lượng phải lớn hơn 0.";
                return RedirectToAction("CartIndex");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                return RedirectToAction("CartIndex");
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Error"] = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho.";
                return RedirectToAction("CartIndex");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId }; // CartId = UserId
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > product.StockQuantity)
                {
                    TempData["Error"] = $"Bạn không thể thêm quá {product.StockQuantity} sản phẩm.";
                    return RedirectToAction("CartIndex");
                }
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = userId, // CartId = UserId
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng!";
            return RedirectToAction("CartIndex");
        }
    }
}
