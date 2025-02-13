using C5.Data;
using C5.Models;
using C5.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace C5.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FastFoodDbContext _context;
        public HomeController(ILogger<HomeController> logger, FastFoodDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 6; // Hiển thị 6 sản phẩm mỗi trang
            int pageNumber = page ?? 1;

            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(); // Lấy danh sách sản phẩm trước

            var pagedProducts = products.ToPagedList(pageNumber, pageSize); // Áp dụng phân trang

            return View(pagedProducts);
        }

        public async Task<IActionResult> DetailsProduct(string id)
        {
            var product = await _context.Products
                .Include(p => p.Category) // Tải luôn danh mục để tránh truy vấn lồng nhau
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category.Name, // Không cần truy vấn lồng nhau
                    Price = p.Price,
                    Image = p.Image,
                    Description = p.Description,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    StatusText = p.IsActive ? "Hoạt động" : "Ngừng bán"
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                TempData["NotFound"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("Index");
            }

            return View(product);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            if (string.IsNullOrEmpty(requestId))
            {
                TempData["Error"] = "Đã xảy ra lỗi không xác định!";
                return RedirectToAction("Index");
            }

            return View(new ErrorViewModel { RequestId = requestId });
        }

    }
}
