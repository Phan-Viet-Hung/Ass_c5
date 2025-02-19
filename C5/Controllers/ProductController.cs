using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.IO;
using X.PagedList.Extensions;

namespace C5.Controllers
{
    public class ProductController : Controller
    {
        private readonly FastFoodDbContext _context;

        public ProductController(FastFoodDbContext context)
        {
            _context = context;
        }

        // Danh sách sản phẩm với phân trang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> ListProduct(int? page)
        {
            int pageNumber = page ?? 1; // Trang mặc định là 1
            int pageSize = 5; // Số sản phẩm trên mỗi trang

            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync(); // Load danh sách trước
            var pagedProducts = products.ToPagedList(pageNumber, pageSize); // Sau đó phân trang

            return View(pagedProducts);
        }

        // Lấy thông tin một sản phẩm theo ID
        [HttpGet]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // View thêm sản phẩm
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // Xử lý thêm sản phẩm
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile imageFile)
        {
            try
            {


                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
                // Xử lý upload ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = UploadImage(imageFile);
                    if (fileName == null)
                    {
                        TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif)!";
                        return View(product);
                    }
                    product.Image = "/uploads/" + fileName;
                }

                // Thiết lập thời gian Việt Nam
                product.CreatedAt = GetVietnamTime();
                
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("ListProduct");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(product);
            }
        }

        // View cập nhật sản phẩm
        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(string id, Product updatedProduct, IFormFile imageFile)
        {
            if (id != updatedProduct.Id) return BadRequest();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();


            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.CategoryId = updatedProduct.CategoryId;
            product.StockQuantity = updatedProduct.StockQuantity;

            // Chỉ cập nhật ảnh nếu có file mới
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = UploadImage(imageFile);
                if (fileName != null)
                {
                    product.Image = "/uploads/" + fileName;
                }
                else
                {
                    TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif)!";
                    return View(updatedProduct);
                }
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("ListProduct");
        }

        // Xóa sản phẩm (Đổi trạng thái)
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["NotFound"] = "Không tìm thấy sản phẩm!";
                return RedirectToAction("ListProduct");
            }

            product.IsActive = !product.IsActive; // Đổi trạng thái thay vì xóa cứng
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật trạng thái sản phẩm!";
            return RedirectToAction("ListProduct");
        }

        // 📌 Hàm hỗ trợ tải ảnh lên
        private string UploadImage(IFormFile imageFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension)) return null;

            var fileName = Guid.NewGuid() + fileExtension;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return fileName;
        }

        // 📌 Hàm lấy giờ Việt Nam
        private static DateTime GetVietnamTime()
        {
            TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        }
    }
}
