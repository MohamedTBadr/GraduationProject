using Domain.Entities;
using Shared;
using System.Linq.Expressions;

namespace Domain.Contracts { 
    public interface IVendorRepository
    {
        Task AddVendorAsync(Vendor vendor, CancellationToken cancellationToken);
        Task DeleteVendorAsync(Vendor vendor, CancellationToken cancellationToken);
        Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PaginatedResponse<Vendor>> GetVendorsAsync(PaginatedRequest paginatedRequest, Expression<Func<Vendor, bool>> visibilityFilter, CancellationToken cancellationToken);
        Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken);
        Task<List<ServiceType>> GetServiceTypesByIdsAsync(List<Guid> ids, CancellationToken cancellationToken);
    }
}