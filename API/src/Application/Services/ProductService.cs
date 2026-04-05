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

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetAllAsync(PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await _ServiceRepository.GetAllAsync(request, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await _ServiceRepository.GetByCategoryIdAsync(categoryId, request, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await _ServiceRepository.GetByVendorIdAsync(vendorId, request, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await _ServiceRepository.GetByServiceTypeIdAsync(ServiceTypeId, request, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }


        public async Task<Result<ServiceDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var Service = await _ServiceRepository.GetByIdAsync(id, cancellationToken);
            if (Service is null)
                return Result<ServiceDTO>.NotFound("Service not found");
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(Service));
        }


        public async Task<Result<ServiceDTO>> CreateAsync(CreateServiceRequest dto, CancellationToken cancellationToken)
        {
            var service = _mapper.Map<Service>(dto);

            if (dto.ServiceImages != null && dto.ServiceImages.Count > 0)
            {
                var images = new List<ServiceImage>();

                foreach (var image in dto.ServiceImages)
                {
                    var imagePath = await _fileService.Upload("Services", image, cancellationToken);
                    images.Add(new ServiceImage
                    {
                        ImagePath = imagePath,
                        ServiceId = service.Id
                    });
                }

                service.ServiceImages = images;
            }

            var created = await _ServiceRepository.CreateAsync(service, cancellationToken);

            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(created));
        }

        public async Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceDTO dto, CancellationToken cancellationToken)
        {
            var exists = await _ServiceRepository.ExistsAsync(dto.Id, cancellationToken);
            if (!exists)
                return Result<ServiceDTO>.NotFound("Service not found");

            var Service = _mapper.Map<Service>(dto);
            var updated = await _ServiceRepository.UpdateAsync(Service, cancellationToken);
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(updated));
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var exists = await _ServiceRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result<bool>.NotFound("Service not found");

            await _ServiceRepository.DeleteAsync(id, cancellationToken);
            await _fileService.DeleteAsync(new List<string> { id.ToString() }, cancellationToken); // Assuming you want to delete associated images as well
            return Result<bool>.Success(true);
        }

         //✅ Fixed
        public async Task<Result<List<ServiceDTO>>> AIFilterAsync(AIRequest AIRequest,CancellationToken cancellationToken)
        {
            var Services = await _ServiceRepository.AIFilterAsync(AIRequest, cancellationToken); // ← awaited
            var mapped = _mapper.Map<List<ServiceDTO>>(Services);
            return Result<List<ServiceDTO>>.Success(mapped);                  // ← async returns Task automatically
        }
    }
}
