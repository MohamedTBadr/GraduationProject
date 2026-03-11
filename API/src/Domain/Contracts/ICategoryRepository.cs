
using Domain.Entities;

namespace Domain.Contracts
{
    public interface ICategoryRepository
    {
        Task AddCategoryAsync(Category category);
        Task DeleteCategoryAsync(Category category);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task UpdateCategoryAsync( Category category);
    }
}