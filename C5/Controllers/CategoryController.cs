using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using C5.Models;
using C5.Service;
using C5.Services;
using C5.Data;

namespace C5.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService service)
        {
            _categoryService = service;
        }

        // Hiển thị danh sách danh mục
        [HttpGet]
        public async Task<IActionResult> ListCategory()
        {
            var categories = await _categoryService.GetAllCategories();
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

            var success = await _categoryService.CreateCategory(category);

            if (!success)
            {
                ModelState.AddModelError("", "Lỗi khi thêm danh mục. API không phản hồi thành công.");
                return View(category);
            }

            TempData["Success"] = "Danh mục đã được thêm!";
            return RedirectToAction(nameof(ListCategory));
        }


        // Trang chỉnh sửa danh mục
        [HttpGet]
        public async Task<IActionResult> EditCategory(string id)
        {
            var category = await _categoryService.GetCategoryById(id);
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

            var success = await _categoryService.UpdateCategory(category.Id, category);
            if (!success)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật danh mục.");
                return View(category);
            }

            TempData["Success"] = "Danh mục đã được cập nhật!";
            return RedirectToAction(nameof(ListCategory));
        }

        // Xóa danh mục
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var success = await _categoryService.DeleteCategory(id);
            if (!success)
            {
                TempData["Error"] = "Không thể xóa danh mục.";
                return RedirectToAction(nameof(ListCategory));
            }

            TempData["Success"] = "Danh mục đã bị xóa.";
            return RedirectToAction(nameof(ListCategory));
        }
    }
}
