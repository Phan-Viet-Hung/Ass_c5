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
                    .Include(c => c.CartItems) // Include thêm Combo
                    .ThenInclude(ci => ci.Combo)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return View(new List<CartItem>());
            }

            return View(cart.CartItems.ToList());
        }


        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(string productId, string comboId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            if ((string.IsNullOrEmpty(productId) && string.IsNullOrEmpty(comboId)) || quantity <= 0)
            {
                TempData["Error"] = "Sản phẩm hoặc combo không hợp lệ, hoặc số lượng phải lớn hơn 0.";
                return RedirectToAction("CartIndex");
            }

            // Lấy giỏ hàng của user
            Cart cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product) // Load sản phẩm
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Combo)   // Load combo
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(productId))
            {
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
                        CartId = userId,
                        ProductId = productId,
                        Product = product, // Gán Product để tránh bị null
                        Quantity = quantity
                    };
                    _context.CartItems.Add(cartItem);
                }
            }
            if (!string.IsNullOrEmpty(comboId))
            {
                var combo = await _context.Combos.FindAsync(comboId);
                if (combo == null || !combo.IsActive)
                {
                    TempData["Error"] = "Combo không tồn tại hoặc đã ngừng bán.";
                    return RedirectToAction("CartIndex");
                }

                if (quantity > combo.StockQuantity)
                {
                    TempData["Error"] = $"Chỉ còn {combo.StockQuantity} combo trong kho.";
                    return RedirectToAction("CartIndex");
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ComboId == comboId);
                if (cartItem != null)
                {
                    if (cartItem.Quantity + quantity > combo.StockQuantity)
                    {
                        TempData["Error"] = $"Bạn không thể thêm quá {combo.StockQuantity} combo.";
                        return RedirectToAction("CartIndex");
                    }
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = userId,
                        ComboId = comboId,
                        Combo = combo, // Gán Combo để tránh bị null
                        Quantity = quantity
                    };
                    _context.CartItems.Add(cartItem);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Sản phẩm/Combo đã được thêm vào giỏ hàng!";
            return RedirectToAction("CartIndex");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCart(string productId, string action)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để cập nhật giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.CartItems.Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.ProductId == productId);

            if (cartItem == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction("CartIndex");
            }

            if (action == "increase")
            {
                if (cartItem.Quantity < cartItem.Product.StockQuantity)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    TempData["Error"] = "Bạn không thể thêm quá số lượng sản phẩm còn lại.";
                }
            }
            else if (action == "decrease")
            {
                cartItem.Quantity--;
                if (cartItem.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("CartIndex");
        }

    }
}
