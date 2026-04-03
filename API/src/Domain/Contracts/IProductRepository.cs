
using Domain.Entities;
using Shared;
namespace Domain.Contracts
{

    public interface IServiceRepository
        {
            Task<Service> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        // PaginatedResult<Service> → PaginatedResponse<Service>
        Task<PaginatedResponse<Service>> GetAllAsync(PaginatedRequest request, CancellationToken cancellationToken);
        Task<PaginatedResponse<Service>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request, CancellationToken cancellationToken);
        Task<PaginatedResponse<Service>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request, CancellationToken cancellationToken);
        Task<PaginatedResponse<Service>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request, CancellationToken cancellationToken);
        Task<Service> CreateAsync(Service Service, CancellationToken cancellationToken);
            Task<Service> UpdateAsync(Service Service, CancellationToken cancellationToken);
            Task DeleteAsync(Guid id, CancellationToken cancellationToken);
            Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);   

        Task<List<Service>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken);
    }
    }
