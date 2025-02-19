using C5.Data;
using C5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;


namespace C5.Controllers
{
    public class ComboController : Controller
    {
        private readonly FastFoodDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ComboController(FastFoodDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult ListCombo(int? page, string search)
        {
            int pageSize = 5; // Số lượng combo trên mỗi trang
            int pageNumber = page ?? 1;

            var combos = _context.Combos
                .Include(c => c.ComboItems)
                .ThenInclude(ci => ci.Product)
                .OrderByDescending(c => c.Id) // Sắp xếp theo thời gian tạo
                .AsQueryable();

            // Tìm kiếm nếu có
            if (!string.IsNullOrEmpty(search))
            {
                combos = combos.Where(c => c.Name.Contains(search));
            }

            var pagedCombos = combos.ToPagedList(pageNumber, pageSize); 

            ViewBag.CurrentSearch = search; // Để lưu lại giá trị tìm kiếm khi phân trang

            return View(pagedCombos);
        }



        [HttpGet]
        public async Task<IActionResult> CreateCombo()
        {
            var products = _context.Products.Where(p => p.IsActive).ToList(); // Lấy sản phẩm còn hàng
            ViewBag.Products = products;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateCombo(Combo combo, IFormFile imageFile, List<string> selectedProducts, List<int> quantities)
        {
                if (imageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    combo.Image = "/uploads/" + fileName;
                }

                _context.Combos.Add(combo);
                await _context.SaveChangesAsync();

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    _context.ComboItems.Add(new ComboItem
                    {
                        ComboId = combo.Id,
                        ProductId = selectedProducts[i],
                        Quantity = quantities[i]
                    });
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("ListCombo");

        }

        public async Task<IActionResult> EditCombo(string id)
        {
            var combo = await _context.Combos.Include(c => c.ComboItems)
                                             .ThenInclude(ci => ci.Product)
                                             .FirstOrDefaultAsync(c => c.Id == id);
            if (combo == null) return NotFound();
            ViewBag.Products = _context.Products.Where(p => p.IsActive).ToList();
            return View(combo);
        }

        [HttpPost]
        public async Task<IActionResult> EditCombo(Combo combo, IFormFile imageFile, List<string> selectedProducts, List<int> quantities)
        {
                var existingCombo = await _context.Combos.Include(c => c.ComboItems)
                                                          .FirstOrDefaultAsync(c => c.Id == combo.Id);
                if (existingCombo == null) return NotFound();

                existingCombo.Name = combo.Name;
                existingCombo.Description = combo.Description;
                existingCombo.Price = combo.Price;
                existingCombo.StockQuantity = combo.StockQuantity;
                existingCombo.IsActive = combo.IsActive;

                if (imageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    existingCombo.Image = "/uploads/" + fileName;
                }

                _context.ComboItems.RemoveRange(existingCombo.ComboItems);

                for (int i = 0; i < selectedProducts.Count; i++)
                {
                    _context.ComboItems.Add(new ComboItem
                    {
                        ComboId = existingCombo.Id,
                        ProductId = selectedProducts[i],
                        Quantity = quantities[i]
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("ListCombo");
        }
        public async Task<IActionResult> ChangeStatus(string id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo == null)
            {
                TempData["NotFound"] = "Không tìm thấy sản phẩm!";
                return RedirectToAction("ListCombo");
            }

            combo.IsActive = !combo.IsActive; // Đổi trạng thái thay vì xóa cứng
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật trạng thái sản phẩm!";
            return RedirectToAction("ListCombo");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCombo(string id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo == null) return NotFound();

            _context.Combos.Remove(combo);
            await _context.SaveChangesAsync();
            return RedirectToAction("ListCombo");
        }
    }
}
