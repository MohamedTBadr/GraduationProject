using Abp.Domain.Repositories;
using Application;
using Application.DTOs.ServiceDTOs;
using Application.Interfaces;
using AutoMapper;

using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Services
{
    public class ServiceService(IServiceRepository _ServiceRepository,IFileService _fileService, IMapper _mapper): IServiceService
    {


        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetAllAsync(
         PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken ct)
        {
            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

            var result = await _ServiceRepository.GetAllAsync(request, visibilityFilter, ct);

            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);

            return Result<PaginatedResponse<ServiceDTO>>.Success(
                new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        private static Expression<Func<Service, bool>> ShowVisibility(PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId)
        {
            Expression<Func<Service, bool>> visibilityFilter = s => !s.IsHidden; // Default

            if (isAdmin && request.IncludeHidden)
            {
                visibilityFilter = s => true; // See everything
            }
            else if (isVendor && userId.HasValue)
            {
                visibilityFilter = s => !s.IsHidden || s.VendorId == userId.Value;
            }

            return visibilityFilter;
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByCategoryIdAsync(
            Guid categoryId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        {
            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

            var result = await _ServiceRepository.GetByCategoryIdAsync(categoryId, request, visibilityFilter, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(
                new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByVendorIdAsync(
            Guid vendorId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        {
            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

            var result = await _ServiceRepository.GetByVendorIdAsync(vendorId, request, visibilityFilter, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(
                new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByServiceTypeIdAsync(
            Guid serviceTypeId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        {
            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

            var result = await _ServiceRepository.GetByServiceTypeIdAsync(serviceTypeId, request, visibilityFilter, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(
                new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }
        public async Task<Result<ServiceDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var Service = await _ServiceRepository.GetByIdAsync(id, cancellationToken);
            if (Service is null)
                return Result<ServiceDTO>.NotFound("Service not found");
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(Service));
        }

        // ServiceAppService.cs (or Command Handler if using MediatR)
        public async Task ToggleStatusAsync(Guid id, CancellationToken ct)
        {
            // 1. Get current status (to toggle it) or pass the desired state
            var service = await _ServiceRepository.GetByIdAsync(id, ct);
            if (service == null) throw new NotFoundException("Service not found");

            var newStatus = !service.IsHidden;

            // 2. Update Database
            await _ServiceRepository.UpdateStatusAsync(id, newStatus, ct);

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

        public async Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceTypeDTO dto, CancellationToken cancellationToken)
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
