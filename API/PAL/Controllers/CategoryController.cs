using API.Controllers;
using BLL.Services;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(ICategoryService categoryService) : BaseController
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
            return Created();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
