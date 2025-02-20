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
using Microsoft.AspNetCore.SignalR;
using C5.Service.VNPay;
using C5.Models.VNPay;
using C5.Service.Momo;

namespace C5.Controllers
{
    public class OrderController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly FastFoodDbContext _context;
        private readonly UserManager<FastFoodUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;
        public OrderController(FastFoodDbContext context, UserManager<FastFoodUser> userManager,IHubContext<NotificationHub> hubContext, IVnPayService vnPayService, IMomoService momoService)
        {
            _hubContext = hubContext;
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _momoService = momoService;
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
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Đơn hàng không hợp lệ hoặc đã được xử lý!";
                return RedirectToAction(nameof(ListOrder));
            }

            order.Status = OrderStatus.Delivering;
            _context.Orders.Update(order);

            var cartItems = await _context.CartItems.Where(c => c.Cart.UserId == order.UserId).ToListAsync();
            foreach (var item in cartItems)
            {
                var product = item.Product;
                if (product.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} không đủ số lượng tồn kho.";
                    return RedirectToAction(nameof(ListOrder));
                }
                product.StockQuantity -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cartItems);

            // Lưu thông báo vào database
            var notification = new Notification
            {
                UserId = order.UserId,
                OrderId = order.Id, // Đảm bảo OrderId đúng
                Message = "Đơn hàng của bạn đã được xác nhận!",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi thông báo real-time, TRUYỀN OrderId chứ không phải message vào link
            await _hubContext.Clients.User(order.UserId)
                .SendAsync("ReceiveNotification", order.Id, notification.Message);

            return RedirectToAction(nameof(ListOrder));
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("ListOrder");
            }

            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng khi đang chờ xác nhận.";
                return RedirectToAction("ListOrder");
            }

            order.Status = OrderStatus.Canceled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đơn hàng đã được hủy thành công.";
            return RedirectToAction("ListOrder");
        }

        public async Task<IActionResult> Checkout(string? VoucherCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.Combo)
                .Where(c => c.Cart.UserId == user.Id)
                .Select(c => new CartItemViewModel
                {
                    ProductName = c.Product != null ? c.Product.Name : c.Combo.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product != null ? c.Product.Price : c.Combo.Price
                })
                .ToListAsync();

            decimal totalAmount = cartItems.Sum(i => i.Quantity * i.UnitPrice);
            decimal discountPercent = 0;
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(VoucherCode))
            {
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == VoucherCode
                                                                              && v.Quantity > 0
                                                                              && v.StartDate < DateTime.UtcNow
                                                                              && v.EndDate > DateTime.UtcNow);
                if (voucher != null)
                {
                    discountPercent = voucher.DiscountPercent;
                    discountAmount = totalAmount * (discountPercent / 100);
                }
            }

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = totalAmount,
                VoucherCode = VoucherCode,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                FinalAmount = totalAmount - discountAmount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string PaymentMethod, string? VoucherCode,OrderInfo MomoModel,PaymentInformationModel vnPayModel )
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.Combo)
                .Where(c => c.Cart.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Checkout");
            }

            decimal totalAmount = cartItems.Sum(i => i.Quantity * (i.Product?.Price ?? i.Combo?.Price ?? 0));
            decimal discountPercent = 0, discountAmount = 0;

            if (!string.IsNullOrEmpty(VoucherCode))
            {
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == VoucherCode && v.Quantity > 0);
                if (voucher != null)
                {
                    discountPercent = voucher.DiscountPercent;
                    discountAmount = totalAmount * (discountPercent / 100);
                    voucher.Quantity--; // Giảm số lượng voucher
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = totalAmount,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                FinalAmount = totalAmount - discountAmount,
                PaymentMethod = PaymentMethod,
                Status = PaymentMethod == "VNPay" ? OrderStatus.WaitingForPayment : OrderStatus.Pending,
                OrderDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                VoucherCode = VoucherCode,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ComboId = c.ComboId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product?.Price ?? c.Combo?.Price ?? 0
                }).ToList()
            };
            //if (PaymentMethod == "VNPay")
            //{
            //    var url = _vnPayService.CreatePaymentUrl(vnPayModel,HttpContext);

            //    return Redirect(url);
            //}
            if(PaymentMethod == "MOMO")
            {
                var response = await _momoService.CreatePaymentAsync(MomoModel);
                return Redirect(response.PayUrl);
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Nếu là VNPay, chuyển hướng đến trang thanh toán VNPay
            
            if(PaymentMethod == null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
        }



        public async Task<IActionResult> OrderSuccess(string orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)  // Load sản phẩm đơn lẻ
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combo)    // Load Combo
                        .ThenInclude(cb => cb.ComboItems) // Load sản phẩm trong Combo
                            .ThenInclude(cp => cp.Product) // Load thông tin sản phẩm trong Combo
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return RedirectToAction("History");

            return View(order);
        }


        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems) // Load cả Combo
                .ThenInclude(oi => oi.Combo)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(string orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin"); // Kiểm tra user có phải admin không

            // Khởi tạo truy vấn lấy Order theo ID
            IQueryable<Order> orderQuery = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combo)
                        .ThenInclude(cb => cb.ComboItems)
                            .ThenInclude(cp => cp.Product);

            if (!isAdmin) // Nếu không phải admin, chỉ lấy đơn hàng của user đó
            {
                orderQuery = orderQuery.Where(o => o.UserId == user.Id);
            }

            var order = await orderQuery.FirstOrDefaultAsync(o => o.Id == orderId); // Đặt điều kiện ID ở đây

            if (order == null) return NotFound();

            return View(order);
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

    }
}
