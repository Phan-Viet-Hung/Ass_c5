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

        public async Task<IActionResult> Index(int? page, string searchCombo, string searchProduct)
        {
            int pageSize = 6; // Hiển thị 6 sản phẩm mỗi trang
            int pageNumber = page ?? 1;

            // Query sản phẩm còn hàng
            var productsQuery = _context.Products
                .Where(p => p.IsActive && p.StockQuantity > 0);

            // Query combo còn hàng
            var combosQuery = _context.Combos
                .Where(c => c.IsActive && c.StockQuantity > 0);

            // Tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchCombo))
            {
                searchCombo = searchCombo.ToLower();
                combosQuery = combosQuery.Where(c => c.Name.ToLower().Contains(searchCombo));
            }

            if (!string.IsNullOrEmpty(searchProduct))
            {
                searchProduct = searchProduct.ToLower();
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(searchProduct));
            }

            // Áp dụng sắp xếp theo bảng chữ cái
            productsQuery = productsQuery.OrderBy(p => p.Name);
            combosQuery = combosQuery.OrderBy(c => c.Name);

            // Thực hiện truy vấn
            var products = await productsQuery.ToListAsync(); // Lấy danh sách sản phẩm
            var combos = await combosQuery.ToListAsync(); // Lấy danh sách combo

            // Phân trang bằng ToPagedList()
            var pagedProducts = products.ToPagedList(pageNumber, pageSize);
            var pagedCombos = combos.ToPagedList(pageNumber, pageSize);

            // Lưu giá trị tìm kiếm để hiển thị lại trên giao diện
            ViewBag.ComboSearch = searchCombo;
            ViewBag.ProductSearch = searchProduct;

            // Kết hợp vào ViewModel
            var model = new HomeViewModel
            {
                Products = pagedProducts,
                Combos = pagedCombos
            };

            return View(model);
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
