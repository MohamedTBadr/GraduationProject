
using Domain.Entities;
using Shared;
namespace Domain.Contracts
{

    public interface IProductRepository
        {
            Task<Product> GetByIdAsync(Guid id);
        // PaginatedResult<Product> → PaginatedResponse<Product>
        Task<PaginatedResponse<Product>> GetAllAsync(PaginatedRequest request);
        Task<PaginatedResponse<Product>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request);
        Task<PaginatedResponse<Product>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request);
        Task<PaginatedResponse<Product>> GetByServiceTypeIdAsync(Guid serviceTypeId, PaginatedRequest request);
        Task<Product> CreateAsync(Product product);
            Task<Product> UpdateAsync(Product product);
            Task DeleteAsync(Guid id);
            Task<bool> ExistsAsync(Guid id);


        Task<List<Product>> AIFilterAsync(AIRequest AIRequest);
    }
    }
