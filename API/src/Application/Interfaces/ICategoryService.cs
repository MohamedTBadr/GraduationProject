using Application.DTOs.CategoryDTOs;


namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(CreateCategoryRequest category, CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> DeleteCategoryAsync(Guid id , CancellationToken cancellationToken);
        Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync(CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    }
}