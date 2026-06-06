using Application.DTOs.ServiceDTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Shared;
using Shared.Exceptions;
using System.Linq.Expressions;

namespace Application.Services
{
    public class ServiceService(
        IServiceRepository _ServiceRepository,
        IFileService _fileService, 
        IMapper _mapper,
        IVendorRepository _vendorRepository,
        ISearchService _searchService) : IServiceService
    {
        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetAllAsync(
           PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken ct)
        {
            List<Guid>? luceneIds = null;
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var serviceIds = await _searchService.SearchServicesAsync(request.SearchTerm);
                luceneIds = serviceIds.ToList();

                if (luceneIds.Count == 0)
                    return Result<PaginatedResponse<ServiceDTO>>.NotFound(404, "No services found matching the criteria.");
            }

            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

            var result = await _ServiceRepository.GetAllAsync(request, visibilityFilter, luceneIds, ct);
            var mappedItems = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            if (!mappedItems.Any())
                return Result<PaginatedResponse<ServiceDTO>>.NotFound(404, "No services found matching the criteria.");

            return Result<PaginatedResponse<ServiceDTO>>.Success(
                new PaginatedResponse<ServiceDTO>(mappedItems, result.TotalCount, result.PageNumber, result.PageSize));
        }
        private static Expression<Func<Service, bool>> ShowVisibility(PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId)
        {
            Expression<Func<Service, bool>> visibilityFilter = s => !s.IsHidden; // Default
            if (isAdmin && request.IncludeHidden)
            {
                visibilityFilter = s => true; // See everything
            }
            return visibilityFilter;
        }


        public async Task<Result<ServiceDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var Service = await _ServiceRepository.GetByIdAsync(id, cancellationToken);
            if (Service is null)
                return Result<ServiceDTO>.NotFound(404, "Service not found");
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(Service));
        }

        public async Task ToggleStatusAsync(Guid id, CancellationToken ct)
        {
            var service = await _ServiceRepository.GetByIdAsync(id, ct);
            if (service == null) throw new NotFoundException("Service not found");

            var newStatus = !service.IsHidden;
            await _ServiceRepository.UpdateStatusAsync(id, newStatus, ct);
        }

        public async Task<Result<ServiceDTO>> CreateAsync(CreateServiceRequest dto, CancellationToken cancellationToken)
        {
            var vendor = await _vendorRepository.GetVendorByIdAsync(dto.VendorId.Value, cancellationToken);
            if (vendor is null)
                return Result<ServiceDTO>.NotFound(404, "Vendor not found");

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
            
            // Index in Lucene
            await _searchService.IndexServiceAsync(created);

            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(created));
        }

        public async Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceDTO dto, CancellationToken cancellationToken)
        {
            var exists = await _ServiceRepository.ExistsAsync(dto.Id, cancellationToken);
            if (!exists)
                return Result<ServiceDTO>.NotFound(404, "Service not found");

            var service = _mapper.Map<Service>(dto);

            if (dto.Images != null && dto.Images.Count > 0)
            {
                var oldImages = await _ServiceRepository.GetServiceImagesAsync(dto.Id, cancellationToken);
                if (oldImages.Any())
                {
                    var oldKeys = oldImages.Select(i => i.ImagePath).ToList();
                    await _fileService.DeleteAsync(oldKeys, cancellationToken);
                    await _ServiceRepository.DeleteServiceImagesAsync(dto.Id, cancellationToken);
                }

                var newImages = new List<ServiceImage>();
                foreach (var file in dto.Images)
                {
                    var url = await _fileService.Upload("services", file, cancellationToken);
                    newImages.Add(new ServiceImage
                    {
                        ServiceId = dto.Id,
                        ImagePath = url
                    });
                }
                service.ServiceImages = newImages;
            }

            var updated = await _ServiceRepository.UpdateAsync(service, cancellationToken);
            
            // Update Lucene Index
            await _searchService.IndexServiceAsync(updated);

            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(updated));
        }

        public async Task AddRatingAsync(ServiceRatingRequest dto, CancellationToken cancellationToken)
        {
            if (dto.UserId is not Guid userId)
                throw new BadRequestException(new List<string> { "User ID is required to add a rating." });

            var hasPurchased = await _ServiceRepository.HasUserPurchasedAsync(dto.UserId.Value, dto.ServiceId, cancellationToken);
            if (!hasPurchased)
                throw new BadRequestException(new List<string> { "User cannot rate a service they did not purchase." });

            var rating = new ServiceRating
            {
                Id = Guid.NewGuid(),
                ServiceId = dto.ServiceId,
                UserId = (Guid)dto.UserId,
                Rating = dto.Rating,
                Review = dto.Review
            };
            await _ServiceRepository.AddRatingAsync(rating, cancellationToken);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var exists = await _ServiceRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result<bool>.NotFound(404, "Service not found");

            await _ServiceRepository.DeleteAsync(id, cancellationToken);
            await _fileService.DeleteAsync(new List<string> { id.ToString() }, cancellationToken);
            
            // Remove from Lucene
            await _searchService.RemoveServiceAsync(id);

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<ServiceDTO>>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken)
        {
            // Phase 1: Use Lucene to get services relevant to the event type within budget
            var luceneIds = await _searchService.SearchServicesAsync(
                query: AIRequest.EventTypeName ?? string.Empty,
                serviceTypeId: null,
                minPrice: null,
                maxPrice: AIRequest.Budget
            );

            var idList = luceneIds.ToList();

            List<Service> services;
            if (idList.Count > 0)
            {
                // Lucene found relevant services — load full entities by ID
                services = await _ServiceRepository.GetByIdsAsync(idList, cancellationToken);
            }
            else
            {
                // Cold start / empty index — fall back to SQL budget filter
                services = await _ServiceRepository.AIFilterAsync(AIRequest, cancellationToken);
            }

            var mapped = _mapper.Map<List<ServiceDTO>>(services);
            return Result<List<ServiceDTO>>.Success(mapped);
        }

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByEventTypeIdAsync(Guid eventTypeId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        {
            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);
            var result = await _ServiceRepository.GetByEventTypeIdAsync(eventTypeId, request, visibilityFilter, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }
        public async Task RebuildSearchIndexAsync()
        {
            // Note: RebuildIndexAsync clears EVERYTHING, so we should be careful if calling both vendor and service rebuilds.
            // Ideally ISearchService.RebuildIndexAsync should take a type filter or we just clear once.
            // For now, we assume this is a full rebuild.
            
            const int pageSize = 200;
            var pageIndex = 1;

            while (true)
            {
                var result = await _ServiceRepository.GetAllAsync(
                    new PaginatedRequest { PageIndex = pageIndex, PageSize = pageSize },
                    s => true,
                    CancellationToken.None);

                foreach (var service in result.Items)
                {
                    await _searchService.IndexServiceAsync(service);
                }

                if (pageIndex * pageSize >= result.TotalCount || !result.Items.Any())
                    break;

                pageIndex++;
            }
        }
    }
}
