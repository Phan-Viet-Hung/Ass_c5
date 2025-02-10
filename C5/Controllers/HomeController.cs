using C5.Data;
using C5.Models;
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
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CategoryName = _context.Categories.Where(c => c.Id == p.CategoryId).Select(c => c.Name).FirstOrDefault(),
                    p.Price,
                    p.Image,
                    p.Description,
                    p.StockQuantity,
                    p.IsActive,
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
