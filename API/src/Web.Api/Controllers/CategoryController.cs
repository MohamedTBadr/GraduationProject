using Application.DTOs.CategoryDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(ICategoryService categoryService) : BaseController
    {
        [HttpGet]
        [HybridCache(1800,"categories")]

        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        [HttpGet("{id}")]
        [HybridCache(1800,"categories/{id}")]

        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [InvalidateCache("categories")]
        public async Task<IActionResult> AddCategory(string name)
        {
            await categoryService.AddCategoryAsync(new CreateCategoryRequest(name));
            return Created();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [InvalidateCache("categories/{id}", "categories")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        [InvalidateCache("categories/{id}", "categories")]
        public async Task<IActionResult> UpdateCategory(Guid id, string name)
        {
            await categoryService.UpdateCategoryAsync(id, new UpdateCategoryRequest(name));
            return Ok();

        }
    }
}
