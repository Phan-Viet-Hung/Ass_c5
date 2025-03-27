using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using C5.Models;
using C5.Data;
using System;

namespace C5.Controllers
{
    public class CategoryController : Controller
    {
        private readonly FastFoodDbContext _context;

        public CategoryController(FastFoodDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách danh mục
        [HttpGet]
        public async Task<IActionResult> ListCategory()
        {
            var categories = await _context.Categories.ToListAsync();
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
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ.");
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Danh mục đã được thêm!";
            return RedirectToAction(nameof(ListCategory));
        }

        // Trang chỉnh sửa danh mục
        [HttpGet]
        public async Task<IActionResult> EditCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(ListCategory));
            }
            return View(category);
        }

        // Xử lý cập nhật danh mục
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Danh mục đã được cập nhật!";
            return RedirectToAction(nameof(ListCategory));
        }

        // Xóa danh mục
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Không thể tìm thấy danh mục.";
                return RedirectToAction(nameof(ListCategory));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Danh mục đã bị xóa.";
            return RedirectToAction(nameof(ListCategory));
        }
    }
}
