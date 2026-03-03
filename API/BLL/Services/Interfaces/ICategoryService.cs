using BLL.DTOs.CategoryDTOs;
using DAL.Entities;

namespace BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(CreateCategoryRequest category);
        Task<Result<CategoryDTO>> DeleteCategoryAsync(Guid id);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id);
        Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    }
}