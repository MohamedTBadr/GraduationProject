using Application.DTOs.ServiceDTOs;

using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IServiceService
    {
        // PaginatedResult<ServiceDto> → PaginatedResponse<ServiceDto>
        Task<Result<PaginatedResponse<ServiceDTO>>> GetAllAsync(PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken);
        //Task<Result<PaginatedResponse<ServiceDTO>>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken);
        Task<Result<PaginatedResponse<ServiceDTO>>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken);
        Task<Result<PaginatedResponse<ServiceDTO>>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken);

        Task<Result<List<ServiceDTO>>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken);
        Task<Result<ServiceDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<ServiceDTO>> CreateAsync(CreateServiceRequest dto, CancellationToken cancellationToken);
        Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceDTO dto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task AddRatingAsync(ServiceRatingRequest dto, CancellationToken cancellationToken);
        Task ToggleStatusAsync(Guid id, CancellationToken ct);
    }
}
