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

        //public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByCategoryIdAsync(
        //    Guid categoryId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        //{
        //    Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

        //    var result = await _ServiceRepository.GetByCategoryIdAsync(categoryId, request, visibilityFilter, cancellationToken);
        //    var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
        //    return Result<PaginatedResponse<ServiceDTO>>.Success(
        //        new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        //}

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
                return Result<ServiceDTO>.NotFound(404, "Service not found");
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

        public async Task<Result<ServiceDTO>> UpdateAsync(UpdateServiceDTO dto, CancellationToken cancellationToken)
        {
            var exists = await _ServiceRepository.ExistsAsync(dto.Id, cancellationToken);
            if (!exists)
                return Result<ServiceDTO>.NotFound(404, "Service not found");

            var service = _mapper.Map<Service>(dto);

            if (dto.Images != null && dto.Images.Count > 0)
            {
                // 1. Get old image keys from DB and delete from S3
                var oldImages = await _ServiceRepository.GetServiceImagesAsync(dto.Id, cancellationToken);
                if (oldImages.Any())
                {
                    var oldKeys = oldImages.Select(i => i.ImagePath).ToList(); // S3 keys
                    await _fileService.DeleteAsync(oldKeys, cancellationToken);
                    await _ServiceRepository.DeleteServiceImagesAsync(dto.Id, cancellationToken);
                }

                // 2. Upload new images to S3
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
            else
            {
                service.ServiceImages = null; // repo will skip images
            }

            var updated = await _ServiceRepository.UpdateAsync(service, cancellationToken);
            return Result<ServiceDTO>.Success(_mapper.Map<ServiceDTO>(updated));
        }

        public async Task AddRatingAsync(ServiceRatingRequest dto, CancellationToken cancellationToken)
        {
            if (dto.UserId is not Guid userId)
            {
                throw new BadRequestException(new List<string> { "User ID is required to add a rating." });
            }


            var hasPurchased = await _ServiceRepository.HasUserPurchasedAsync(dto.UserId.Value, dto.ServiceId, cancellationToken);
            if (!hasPurchased)
                throw new BadRequestException(new List<string> { "User cannot rate a service they did not purchase." });


          
            var rating = new ServiceRating
            {
                Id = Guid.NewGuid(),
                ServiceId = dto.ServiceId,
                UserId = (Guid)dto.UserId, // Handle null user ID as needed
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

        public async Task<Result<PaginatedResponse<ServiceDTO>>> GetByEventTypeIdAsync(Guid eventTypeId, PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        {

            Expression<Func<Service, bool>> visibilityFilter = ShowVisibility(request, isAdmin, isVendor, userId);

            var result = await _ServiceRepository.GetByEventTypeIdAsync(eventTypeId, request, visibilityFilter, cancellationToken);
            var mapped = _mapper.Map<IEnumerable<ServiceDTO>>(result.Items);
            return Result<PaginatedResponse<ServiceDTO>>.Success(
                new PaginatedResponse<ServiceDTO>(mapped, result.TotalCount, result.PageNumber, result.PageSize));
        }
        }
    }
