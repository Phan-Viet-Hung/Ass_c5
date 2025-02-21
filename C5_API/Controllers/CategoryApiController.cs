using C5.Models;
using C5.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace C5.ApiControllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryApiController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            return Ok(await _categoryService.GetAllCategories());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(string id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] Category category)
        {
            var result = await _categoryService.CreateCategory(category);
            if (!result) return BadRequest("Lỗi khi thêm danh mục.");
            return Ok(new { message = "Thêm danh mục thành công!" });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(string id, [FromBody] Category updatedCategory)
        {
            var result = await _categoryService.UpdateCategory(id, updatedCategory);
            if (!result) return BadRequest("Lỗi khi cập nhật danh mục.");
            return Ok(new { message = "Cập nhật danh mục thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(string id)
        {
            var result = await _categoryService.DeleteCategory(id);
            if (!result) return NotFound("Không tìm thấy danh mục.");
            return Ok(new { message = "Xóa danh mục thành công!" });
        }
    }
}
