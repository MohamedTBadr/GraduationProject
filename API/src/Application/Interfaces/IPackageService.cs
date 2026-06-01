using Application.DTOs.PackageDTOs;
using Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPackageService
    {
        Task<Result<PaginatedResponse<PackageDTO>>> GetAllAsync(PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken);
        Task<Result<PackageDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<PackageDTO>> CreateAsync(CreatePackageDTO dto, CancellationToken cancellationToken);
        Task<Result<PackageDTO>> UpdateAsync(UpdatePackageDTO dto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
