using Application;
using Application.DTOs.VendorDTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Application.Services
{
    public class VendorService(UserManager<ApplicationUser> userManager, IVendorRepository vendorRepository, ApplicationDbContext dbContext, IMapper mapper) : IVendorService
    {
        public async Task<Result<List<VendorListDTO>>> GetVendorsAsync()
        {
            // Simulate fetching vendors from a database or service
            var vendors = await vendorRepository.GetVendorsAsync(); 
            var vendorListDTOs = mapper.Map<List<VendorListDTO>>(vendors);
            return Result<List<VendorListDTO>>.Success(vendorListDTOs);
        }

        public async Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound("Vendor not found");
            }
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> AddVendorAsync(CreateVendorRequest request)
        {
      
    
            // 2. Create the Vendor linked to that user
            var serviceTypeIds = request.ServiceTypes.Select(s => s.Id).ToList();

            // Fetch real ServiceType entities from DB
            var existingServiceTypes = await dbContext.ServiceTypes
                .Where(s => serviceTypeIds.Contains(s.Id))
                .ToListAsync();

            if (existingServiceTypes.Count != serviceTypeIds.Count)
                return Result<VendorDetailsDTO>.Failure(ErrorType.NotFound, "One or more ServiceTypes not found.");


            // 1. Create the ApplicationUser via Identity
            var user = new ApplicationUser
            {
                Id = new Guid(),
                UserName = request.Name,
                Email = request.Email,
                PhoneNumber = request.Phone,
            };

            var identityResult = await userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return Result<VendorDetailsDTO>.Failure(ErrorType.AlreadyExists, errors);
            }

            await userManager.AddToRoleAsync(user, "Vendor");


            var vendor = new Vendor
            {
                UserId = user.Id,
                User = user,
                BusinessName = request.BusinessName,
                YearsInBusiness = request.YearsInBusiness,
                Description = request.Description,
                PortfolioLink = request.PortfolioLink,
                Address = request.Address,
                IsVerified = false,
                VendorServiceTypes = existingServiceTypes.Select(s => new VendorServiceType
                {
                    ServiceTypeId = s.Id,
                    ServiceType = s       // ✅ tracked entity, EF won't re-insert it
                }).ToList()
            };

            await vendorRepository.AddVendorAsync(vendor);

            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request)
        {
            var existingVendor = await vendorRepository.GetVendorByIdAsync(id);
            if (existingVendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound("Vendor not found");
            }
            var vendorMapped = mapper.Map(request, existingVendor);
            await vendorRepository.UpdateVendorAsync(vendorMapped);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendorMapped);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> DeleteVendorAsync(Guid id)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound("Vendor not found");
            }

            await vendorRepository.DeleteVendorAsync(vendor);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);

        }

        public async Task<Result<VendorDetailsDTO>> ApproveVendorAsync(Guid id)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound("Vendor not found");
            }
            vendor.IsVerified = true;
            await vendorRepository.UpdateVendorAsync(vendor);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public Task<Result<VendorDetailsDTO>> RateVendorAsync(Guid id, RatingVendorRequest request)
        { 
            return Task.FromResult(Result<VendorDetailsDTO>.Success(new VendorDetailsDTO()));

        }
    }
}
