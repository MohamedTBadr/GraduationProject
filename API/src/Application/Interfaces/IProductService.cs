using Application.DTOs.ProductDTOs;

using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IProductService
    {
        // PaginatedResult<ProductDto> → PaginatedResponse<ProductDto>
        Task<Result<PaginatedResponse<ProductDTO>>> GetAllAsync(PaginatedRequest request);
        Task<Result<PaginatedResponse<ProductDTO>>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request);
        Task<Result<PaginatedResponse<ProductDTO>>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request);
        Task<Result<PaginatedResponse<ProductDTO>>> GetByServiceTypeIdAsync(Guid serviceTypeId, PaginatedRequest request);

        Task<Result<List<ProductDTO>>> AIFilterAsync(AIRequest AIRequest);
        Task<Result<ProductDTO>> GetByIdAsync(Guid id);
        Task<Result<ProductDTO>> CreateAsync(CreateProductRequest dto);
        Task<Result<ProductDTO>> UpdateAsync(UpdateProductDTO dto);
        Task<Result<bool>> DeleteAsync(Guid id);
    }
}
