using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Mvc;

namespace C5.Controllers
{
    public class CategoryController : Controller
    {
        private readonly FastFoodDbContext _context;
        public CategoryController(FastFoodDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult ListCategory()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(Category ct)
        {
            _context.Categories.Add(ct);
            _context.SaveChanges();
            return RedirectToAction("ListCategory", "Category"); //  Đảm bảo về đúng Controller
        }

        [HttpGet]
        public IActionResult EditCategory(string id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                TempData["NotFound"] = "Không tìm thấy danh mục này";
                return RedirectToAction("ListCategory");
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category ct)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == ct.Id);
            if (category == null)
            {
                TempData["NotFound"] = "Không tìm thấy danh mục này";
                return RedirectToAction("ListCategory");
            }

            category.Name = ct.Name; // Chỉ cập nhật trường cần thiết
            _context.SaveChanges();
            return RedirectToAction("ListCategory");
        }

        public IActionResult DeleteCategory(string id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                TempData["NotFound"] = "Không tìm thấy danh mục này";
                return RedirectToAction("ListCategory");
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("ListCategory");
        }
    }
}
