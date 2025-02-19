using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using C5.Models;
using C5.Data;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> AddCategory([Bind("Id, Name")] Category ct)
    {
        if (!ModelState.IsValid)
        {
            return View(ct);
        }

        _context.Categories.Add(ct);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Danh mục đã được thêm thành công!";
        return RedirectToAction(nameof(ListCategory));
    }

    // Trang chỉnh sửa danh mục
    [HttpGet]
    public async Task<IActionResult> EditCategory(string id)
    {
        if (id == null)
        {
            return RedirectToAction(nameof(ListCategory));
        }

        var category = await _context.Categories.FindAsync(id);
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

        category.Name = ct.Name;
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
            TempData["Error"] = "Không tìm thấy danh mục.";
            return RedirectToAction(nameof(ListCategory));
        }
        // Tìm tất cả sản phẩm thuộc danh mục này
        var productsInCategory = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();

        // Đặt tất cả sản phẩm này thành hết hàng
        foreach (var product in productsInCategory)
        {
            product.IsActive = false;
            product.CategoryId = null; // Xóa liên kết danh mục
        }
        // Cập nhật database
        _context.Products.UpdateRange(productsInCategory);
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Danh mục đã bị xóa.";
        return RedirectToAction(nameof(ListCategory));
        }
    }
}
