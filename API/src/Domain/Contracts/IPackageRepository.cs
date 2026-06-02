using Domain.Entities;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IPackageRepository
    {
        Task<Package> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PaginatedResponse<Package>> GetAllAsync(PaginatedRequest request, Expression<Func<Package, bool>> visibilityFilter, CancellationToken ct);
        Task<Package> CreateAsync(Package package, CancellationToken cancellationToken);
        Task<Package> UpdateAsync(Package package, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    }
}
