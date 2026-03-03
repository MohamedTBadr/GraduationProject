using AutoMapper;
using BLL.DTOs;
using BLL.DTOs.ProductDTOs;
using BLL.Services.Interfaces;
using Common;
using DAL.Entities;
using DAL.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class ProductService(IProductRepository _productRepository, IMapper _mapper): IProductService
    {

        public async Task<Result<PaginatedResponse<ProductDTO>>> GetAllAsync(PaginatedRequest request)
        {
            var result = await _productRepository.GetAllAsync(request);
            var mapped = _mapper.Map<IEnumerable<ProductDTO>>(result.Items);
            return Result<PaginatedResponse<ProductDTO>>.Success(new PaginatedResponse<ProductDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ProductDTO>>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request)
        {
            var result = await _productRepository.GetByCategoryIdAsync(categoryId, request);
            var mapped = _mapper.Map<IEnumerable<ProductDTO>>(result.Items);
            return Result<PaginatedResponse<ProductDTO>>.Success(new PaginatedResponse<ProductDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ProductDTO>>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request)
        {
            var result = await _productRepository.GetByVendorIdAsync(vendorId, request);
            var mapped = _mapper.Map<IEnumerable<ProductDTO>>(result.Items);
            return Result<PaginatedResponse<ProductDTO>>.Success(new PaginatedResponse<ProductDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ProductDTO>>> GetByServiceTypeIdAsync(Guid serviceTypeId, PaginatedRequest request)
        {
            var result = await _productRepository.GetByServiceTypeIdAsync(serviceTypeId, request);
            var mapped = _mapper.Map<IEnumerable<ProductDTO>>(result.Items);
            return Result<PaginatedResponse<ProductDTO>>.Success(new PaginatedResponse<ProductDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }


        public async Task<Result<ProductDTO>> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return Result<ProductDTO>.NotFound("Product not found");
            return Result<ProductDTO>.Success(_mapper.Map<ProductDTO>(product));
        }
      

        public async Task<Result<ProductDTO>> CreateAsync(CreateProductRequest dto)
        {
            var product = _mapper.Map<Product>(dto);
            var created = await _productRepository.CreateAsync(product);
            return Result<ProductDTO>.Success(_mapper.Map<ProductDTO>(created));
        }

        public async Task<Result<ProductDTO>> UpdateAsync(UpdateProductDTO dto)
        {
            var exists = await _productRepository.ExistsAsync(dto.Id);
            if (!exists)
                return Result<ProductDTO>.NotFound("Product not found");

            var product = _mapper.Map<Product>(dto);
            var updated = await _productRepository.UpdateAsync(product);
            return Result<ProductDTO>.Success(_mapper.Map<ProductDTO>(updated));
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
                return Result<bool>.NotFound("Product not found");

            await _productRepository.DeleteAsync(id);
            return Result<bool>.Success(true);
        }
    }
}
