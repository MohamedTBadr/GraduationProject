using Application;
using Application.DTOs.VendorDTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.Linq.Expressions;


namespace Application.Services
{
    public class VendorService(IUserRepository userRepository, UserManager<ApplicationUser> userManager,IVendorRepository vendorRepository, IEventItemRepository _eventItemRepository, IMapper mapper, IFileService _fileService) : IVendorService
    {
        public async Task<Result<PaginatedResponse<VendorListDTO>>> GetVendorsAsync(
      PaginatedRequest paginatedRequest,
      bool isAdmin,
      CancellationToken cancellationToken)
        {
            // ✅ Admin sees all, others only see verified
            Expression<Func<Vendor, bool>> visibilityFilter = isAdmin
                ? v => true
                : v => v.IsVerified;

            var vendors = await vendorRepository.GetVendorsAsync(
                paginatedRequest,
                visibilityFilter,
                cancellationToken);

            var mappedItems = mapper.Map<List<VendorListDTO>>(vendors.Items);
            var response = new PaginatedResponse<VendorListDTO>(
                mappedItems,
                vendors.TotalCount,
                vendors.PageNumber,
                vendors.PageSize);

            return Result<PaginatedResponse<VendorListDTO>>.Success(response);
        }

        public async Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404,"Vendor not found");
            }
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }
        public async Task<List<VendorBookingDto>> GetVendorBookingsAsync(
                   Guid vendorId,
                   CancellationToken cancellationToken = default)
        {
            var bookings = await _eventItemRepository.GetVendorBookingsAsync(vendorId, cancellationToken);

            return bookings.Select(ei => new VendorBookingDto
            {
                EventItemId = ei.Id,
                ServiceName = ei.ServiceName,
                Price = ei.Price,
                BookingStatus = ei.ItemStatus,
                Notes = ei.Event.Notes,

                EventId = ei.EventId,
                EventTitle = ei.Event.Title,
                EventType = ei.Event.EventType?.Name ?? string.Empty,
                EventDate = ei.Event.EventDate,
                EventStatus = ei.Event.EventStatus,
                GuestCount = ei.Event.GuestCount,
                Location = ei.Event.Location?.ToString() ?? string.Empty
            }).ToList();
        }
        public async Task<Result<VendorDetailsDTO>> AddVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken)
        {

            // 1. Create the ApplicationUser via Identity
            var user = new ApplicationUser
            {
                Id = new Guid(),
                FirstName = request.FirstName,
                LastName =request.LastName,
                UserName = request.Name,
                Email = request.Email,
                PhoneNumber = request.Phone,
               
            };

            var identityResult = await userRepository.CreateAsync(user, request.Password, "Vendor", cancellationToken);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return Result<VendorDetailsDTO>.Failure(new Error(ErrorType.AlreadyExists, 409, errors));
            }


            var profilePicture = await _fileService.Upload("Vendors", request.ProfilePicture, cancellationToken);
            var document = await _fileService.Upload("VendorDocuments", request.Document, cancellationToken);
            var vendor = new Vendor
            {
                UserId = user.Id,
                BusinessName = request.BusinessName,
                YearsInBusiness = request.YearsInBusiness,
                Description = request.Description,
                PortfolioLink = request.PortfolioLink,
                Address = request.Address,
                IsVerified = false,
                VendorTypeId = request.VendorTypeId,
                ProfilePicture = profilePicture,
                Document = document,
                 ServiceAreas = request.ServiceAreas?.Select(sa => new ServiceArea
                {
                        City = sa.City,
                        Region = sa.Region,
                        Latitude = sa.Lattitude,
                        Longitude = sa.Longitude
                 }).ToList()

            };

            await vendorRepository.AddVendorAsync(vendor, cancellationToken);
            await userManager.AddToRoleAsync(user, "Vendor");

            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken)
        {
            var existingVendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (existingVendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404, "Vendor not found");
            }
            var vendorMapped = mapper.Map(request, existingVendor);
            await vendorRepository.UpdateVendorAsync(vendorMapped, cancellationToken);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendorMapped);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> DeleteVendorAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404, "Vendor not found");
            }

            await vendorRepository.DeleteVendorAsync(vendor, cancellationToken);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);

        }

        public async Task<Result<VendorDetailsDTO>> ApproveVendorAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404, "Vendor not found");
            }
            vendor.IsVerified = true;
            await vendorRepository.UpdateVendorAsync(vendor, cancellationToken);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public Task<Result<VendorDetailsDTO>> RateVendorAsync(Guid id, RatingVendorRequest request, CancellationToken cancellationToken)
        { 
            return Task.FromResult(Result<VendorDetailsDTO>.Success(new VendorDetailsDTO()));

        }
    }
}
