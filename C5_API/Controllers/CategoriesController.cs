using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using C5.Data;
using C5.Models;

namespace C5_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly FastFoodDbContext _context;

        public CategoriesController(FastFoodDbContext context)
        {
            _context = context;
        }

        // 🟢 Lấy danh sách danh mục
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products) // Chỉ lấy sản phẩm trong danh mục
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    Products = c.Products.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Image,
                        p.IsActive
                    })
                })
                .ToListAsync();

            return Ok(categories);
        }


        // 🟢 Lấy chi tiết danh mục theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(string id)
        {
            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return category;
        }

        // 🟡 Cập nhật danh mục
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(string id, Category updatedCategory)
        {
            if (id != updatedCategory.Id)
            {
                return BadRequest();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Cập nhật từng thuộc tính thay vì gán toàn bộ đối tượng
            category.Name = updatedCategory.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // 🟢 Thêm danh mục
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            if (CategoryExists(category.Id))
            {
                return Conflict("Danh mục đã tồn tại.");
            }

            _context.Categories.Add(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Lỗi khi thêm danh mục.");
            }

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // 🔴 Xóa danh mục (kiểm tra sản phẩm trước khi xóa)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có sản phẩm nào thuộc danh mục này không
            var productsInCategory = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();
            if (productsInCategory.Any())
            {
                // Đánh dấu sản phẩm là hết hàng và bỏ liên kết danh mục
                foreach (var product in productsInCategory)
                {
                    product.IsActive = false;
                    product.CategoryId = null;
                }
                _context.Products.UpdateRange(productsInCategory);
            }

            _context.Categories.Remove(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Không thể xóa danh mục này vì có dữ liệu liên quan.");
            }

            return NoContent();
        }

        private bool CategoryExists(string id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
