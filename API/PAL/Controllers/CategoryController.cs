using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(CategoryService categoryService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            await categoryService.AddCategoryAsync(new BLL.DTOs.CategoryDTOs.CreateCategoryRequest(name));
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await categoryService.DeleteCategoryAsync(id);
            return Ok();
        }
    }
}
