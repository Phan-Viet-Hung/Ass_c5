using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace C5.Controllers
{
    public class ProductController : Controller
    {
        private readonly FastFoodDbContext _context;

        public ProductController(FastFoodDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách sản phẩm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> ListProduct()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // Lấy thông tin một sản phẩm theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return product;
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = new SelectList(_context.Categories.Select(c => new { c.Id, c.Name }), "Id", "Name");
            return View();
        }
        public async Task<IActionResult> AddProduct(Product product, IFormFile imageFile)
        {
            try
            {
                

                // Kiểm tra thư mục uploads có tồn tại không, nếu chưa thì tạo
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Kiểm tra file ảnh trước khi lưu
                if (imageFile != null && imageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif)!";
                        ViewBag.Categories = new SelectList(_context.Categories.Select(c => new { c.Id, c.Name }), "Id", "Name");
                        return View(product);
                    }

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Kiểm tra file có được lưu hay không trước khi gán vào Image
                    if (System.IO.File.Exists(filePath))
                    {
                        product.Image = "/uploads/" + fileName;
                    }
                    else
                    {
                        TempData["Error"] = "Lỗi lưu ảnh, vui lòng thử lại!";
                        return View(product);
                    }
                }
                //if (!ModelState.IsValid)
                //{
                //    TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại!";
                //    ViewBag.Categories = new SelectList(_context.Categories.Select(c => new { c.Id, c.Name }), "Id", "Name");
                //    return View(product);
                //}

                // Thiết lập CreatedAt theo giờ Việt Nam
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                product.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("ListProduct");

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                ViewBag.Categories = new SelectList(_context.Categories.Select(c => new { c.Id, c.Name }), "Id", "Name");
                return View(product);
            }
        }




        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.Select(c => new { c.Id, c.Name }), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Sửa thông tin sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(string id, Product updatedProduct, IFormFile imageFile)
        {
            if (id != updatedProduct.Id)
                return BadRequest();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.CategoryId = updatedProduct.CategoryId;

            // Xử lý upload ảnh mới nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.Image = "/uploads/" + fileName;
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("ListProduct");
        }

        // Xóa sản phẩm (đổi trạng thái)
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return View(TempData["NotFound"]);

            product.IsActive = !product.IsActive; // Đổi trạng thái thay vì xóa cứng
            await _context.SaveChangesAsync();

            return RedirectToAction("ListProduct","Product");
        }
    }
}
