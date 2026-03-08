using Application.Interfaces;
using AutoMapper;
using BLL.DTOs.VendorDTOs;
using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Contracts;


namespace BLL.Services
{
    public class VendorService(IVendorRepository vendorRepository, IMapper mapper) : IVendorService
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
            var newVendor = mapper.Map<Vendor>(request);
            await vendorRepository.AddVendorAsync(newVendor);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(newVendor);
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
            return Result<VendorDetailsDTO>.NotFound("Vendor not found");

            await vendorRepository.DeleteVendorAsync(vendor);
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);

        }

    }
}
