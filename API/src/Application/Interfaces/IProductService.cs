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
        Task<Result<PaginatedResponse<ServiceDTO>>> GetAllAsync(PaginatedRequest request);
        Task<Result<PaginatedResponse<ServiceDTO>>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request);
        Task<Result<PaginatedResponse<ServiceDTO>>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request);
        Task<Result<PaginatedResponse<ServiceDTO>>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request);

        Task<Result<List<ServiceDTO>>> AIFilterAsync(AIRequest AIRequest);
        Task<Result<ServiceDTO>> GetByIdAsync(Guid id);
        Task<Result<ServiceDTO>> CreateAsync(CreateServiceRequest dto);
        Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceDTO dto);
        Task<Result<bool>> DeleteAsync(Guid id);
    }
}
