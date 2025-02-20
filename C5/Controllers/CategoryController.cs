using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using C5.Models;
using C5.Data;

namespace C5.Controllers
{
    public class CategoryController : Controller
    {
        private readonly FastFoodDbContext _context;

        public CategoryController(FastFoodDbContext context)
        {
            _context = context;
        }

        // 🟢 Hiển thị danh sách danh mục từ API
        [HttpGet]
        public async Task<IActionResult> ListCategory()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return View(categories);
        }

        // Trang thêm danh mục
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        // Xử lý thêm danh mục
        [HttpPost]
        public async Task<IActionResult> AddCategory([Bind("Id, Name")] Category ct)
        {
            if (!ModelState.IsValid)
            {
                return View(ct);
            }

            try
            {
                _context.Categories.Add(ct);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Danh mục đã được thêm thành công!";
                return RedirectToAction(nameof(ListCategory));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Có lỗi khi lưu dữ liệu. Vui lòng thử lại.");
                Console.WriteLine(ex);
                return View(ct);
            }
        }

        // Trang chỉnh sửa danh mục
        [HttpGet]
        public async Task<IActionResult> EditCategory(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(ListCategory));
            }

            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục này.";
                return RedirectToAction(nameof(ListCategory));
            }

            return View(category);
        }

        // Xử lý cập nhật danh mục
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category ct)
        {
            if (!ModelState.IsValid)
            {
                return View(ct);
            }

            var category = await _context.Categories.FindAsync(ct.Id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục này.";
                return RedirectToAction(nameof(ListCategory));
            }

            try
            {
                category.Name = ct.Name;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Danh mục đã được cập nhật!";
                return RedirectToAction(nameof(ListCategory));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Có lỗi khi cập nhật. Vui lòng thử lại.");
                Console.WriteLine(ex);
                return View(ct);
            }
        }

        // Xóa danh mục
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(ListCategory));
            }

            // Tìm tất cả sản phẩm thuộc danh mục này
            var productsInCategory = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();

            if (productsInCategory.Any())
            {
                // Đánh dấu sản phẩm là hết hàng và hủy liên kết danh mục
                foreach (var product in productsInCategory)
                {
                    product.IsActive = false;
                    product.CategoryId = null;
                }
                _context.Products.UpdateRange(productsInCategory);
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Danh mục đã bị xóa.";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Không thể xóa danh mục này vì có dữ liệu liên quan.";
                Console.WriteLine(ex);
            }

            return RedirectToAction(nameof(ListCategory));
        }
    }
}
