using C5.Data;
using C5.Models;
using Microsoft.EntityFrameworkCore;

namespace C5.Services
{
    public class CategoryService
    {
        private readonly FastFoodDbContext _context;

        public CategoryService(FastFoodDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách danh mục
        public async Task<List<Category>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        // Lấy danh mục theo ID
        public async Task<Category?> GetCategoryById(string id)
        {
            return await _context.Categories.FindAsync(id);
        }

        // Thêm danh mục mới
        public async Task<string> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return "Danh mục đã được thêm thành công!";
        }

        // Cập nhật danh mục
        public async Task<string> UpdateCategory(Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                return "Không tìm thấy danh mục này.";
            }

            existingCategory.Name = category.Name;
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return "Danh mục đã được cập nhật!";
        }

        // Xóa danh mục
        public async Task<string> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return "Không tìm thấy danh mục.";
            }

            // Tìm sản phẩm thuộc danh mục
            var productsInCategory = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();

            // Đặt sản phẩm thành hết hàng và xóa liên kết danh mục
            foreach (var product in productsInCategory)
            {
                product.IsActive = false;
                product.CategoryId = null;
            }

            _context.Products.UpdateRange(productsInCategory);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return "Danh mục đã bị xóa.";
        }
    }
}
