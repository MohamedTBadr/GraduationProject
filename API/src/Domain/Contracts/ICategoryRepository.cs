
using Domain.Entities;

namespace Domain.Contracts
{
    public interface ICategoryRepository
    {
        Task AddCategoryAsync(Category category,CancellationToken cancellationToken);
        Task DeleteCategoryAsync(Category category, CancellationToken cancellationToken);
        Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken);
        Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateCategoryAsync( Category category, CancellationToken cancellationToken);
    }
}