using Application;
using Application.DTOs.ServiceDTOs;
using Application.Interfaces;
using AutoMapper;

using Domain.Contracts;
using Domain.Entities;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ServiceService(IServiceRepository _ServiceRepository,IFileService _fileService, IMapper _mapper): IServiceService
    {

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetAllAsync(PaginatedRequest request)
        {
            var result = await _ServiceRepository.GetAllAsync(request);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request)
        {
            var result = await _ServiceRepository.GetByCategoryIdAsync(categoryId, request);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request)
        {
            var result = await _ServiceRepository.GetByVendorIdAsync(vendorId, request);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request)
        {
            var result = await _ServiceRepository.GetByServiceTypeIdAsync(ServiceTypeId, request);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }


        public async Task<Result<ServiceDTO>> GetByIdAsync(Guid id)
        {
            var Service = await _ServiceRepository.GetByIdAsync(id);
            if (Service is null)
                return Result<ServiceDTO>.NotFound("Service not found");
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(Service));
        }


        public async Task<Result<ServiceDTO>> CreateAsync(CreateServiceRequest dto)
        {
            var service = _mapper.Map<Service>(dto);

            if (dto.ServiceImages != null && dto.ServiceImages.Count > 0)
            {
                var images = new List<ServiceImage>();

                foreach (var image in dto.ServiceImages)
                {
                    var imagePath = await _fileService.Upload("Services", image);
                    images.Add(new ServiceImage
                    {
                        ImagePath = imagePath,
                        ServiceId = service.Id
                    });
                }

                service.ServiceImages = images;
            }

            var created = await _ServiceRepository.CreateAsync(service);

            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(created));
        }

        public async Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceDTO dto)
        {
            var exists = await _ServiceRepository.ExistsAsync(dto.Id);
            if (!exists)
                return Result<ServiceDTO>.NotFound("Service not found");

            var Service = _mapper.Map<Service>(dto);
            var updated = await _ServiceRepository.UpdateAsync(Service);
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(updated));
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            var exists = await _ServiceRepository.ExistsAsync(id);
            if (!exists)
                return Result<bool>.NotFound("Service not found");

            await _ServiceRepository.DeleteAsync(id);
            await _fileService.DeleteAsync(new List<string> { id.ToString() }); // Assuming you want to delete associated images as well
            return Result<bool>.Success(true);
        }

         //✅ Fixed
        public async Task<Result<List<ServiceDTO>>> AIFilterAsync(AIRequest AIRequest)
        {
            var Services = await _ServiceRepository.AIFilterAsync(AIRequest); // ← awaited
            var mapped = _mapper.Map<List<ServiceDTO>>(Services);
            return Result<List<ServiceDTO>>.Success(mapped);                  // ← async returns Task automatically
        }
    }
}
