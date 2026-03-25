
using Domain.Entities;
using Shared;
namespace Domain.Contracts
{

    public interface IServiceRepository
        {
            Task<Service> GetByIdAsync(Guid id);
        // PaginatedResult<Service> → PaginatedResponse<Service>
        Task<PaginatedResponse<Service>> GetAllAsync(PaginatedRequest request);
        Task<PaginatedResponse<Service>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request);
        Task<PaginatedResponse<Service>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request);
        Task<PaginatedResponse<Service>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request);
        Task<Service> CreateAsync(Service Service);
            Task<Service> UpdateAsync(Service Service);
            Task DeleteAsync(Guid id);
            Task<bool> ExistsAsync(Guid id);


        Task<List<Service>> AIFilterAsync(AIRequest AIRequest);
    }
    }
