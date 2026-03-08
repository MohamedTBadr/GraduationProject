using Application.DTOs.CategoryDTOs;
using BLL.DTOs.CategoryDTOs;
using DAL.Entities;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(CreateCategoryRequest category);
        Task<Result<CategoryDTO>> DeleteCategoryAsync(Guid id);
        Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync();
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id);
        Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    }
}