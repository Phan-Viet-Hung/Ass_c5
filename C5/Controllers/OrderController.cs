using C5.Models.ViewModels;
using C5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using C5.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using X.PagedList;
using X.PagedList.Mvc.Core;
using static C5.Models.Order;
using X.PagedList.Extensions;

namespace C5.Controllers
{
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _context;
        private readonly UserManager<FastFoodUser> _userManager;

        public OrderController(FastFoodDbContext context, UserManager<FastFoodUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin")] // Chỉ cho phép người dùng Admin đã đăng nhập
                                     // Danh sách đơn hàng với phân trang
        public ActionResult ListOrder(int page = 1, int pageSize = 5)
        {
            var orders =  _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToPagedList(page, pageSize);

            return View(orders);
        }

        // Xác nhận đơn hàng (Chuyển trạng thái từ Pending -> Delivering)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmOrder(string orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm nếu cần
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatus.Pending)
            {
                // Chuyển trạng thái thành "Đang giao"
                order.Status = OrderStatus.Delivering;
                _context.Orders.Update(order);

                // Xóa sản phẩm trong giỏ hàng của user này
                var cartItems = await _context.CartItems
                    .Where(c => c.Cart.UserId == order.UserId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ListOrder));
        }


        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Where(c => c.Cart.UserId == user.Id)
                .Select(c => new CartItemViewModel
                {
                    ProductName = c.Product.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                }).ToListAsync();

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(i => i.Quantity * i.UnitPrice)
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string PaymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Product) // Đảm bảo Product được load
                .Where(c => c.Cart.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Checkout");
            }

            // Kiểm tra sản phẩm nào bị null
            if (cartItems.Any(i => i.Product == null))
            {
                TempData["Error"] = "Có sản phẩm không hợp lệ trong giỏ hàng. Vui lòng kiểm tra lại.";
                return RedirectToAction("Checkout");
            }

            var totalAmount = cartItems.Sum(i => i.Quantity * i.Product.Price);

            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = totalAmount,
                PaymentMethod = PaymentMethod,
                Status = 0, // Chờ xác nhận
                OrderDate = DateTime.UtcNow,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price,
                    CartItemId = c.Id // Lưu lại ID giỏ hàng
                }).ToList()
            };


            _context.Orders.Add(order);

            // Xóa giỏ hàng sau khi đặt hàng thành công
            //_context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
        }

        public async Task<IActionResult> OrderSuccess(string orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Load sản phẩm từ OrderItem
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [Authorize] // Yêu cầu đăng nhập
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate) // Sắp xếp theo thời gian mới nhất
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm
                .ToListAsync();

            return View(orders);
        }
        [Authorize] // Yêu cầu đăng nhập
        public async Task<IActionResult> OrderDetails(string orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == user.Id) // Đảm bảo user chỉ xem đơn hàng của mình
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Load thông tin sản phẩm
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

    }
}
