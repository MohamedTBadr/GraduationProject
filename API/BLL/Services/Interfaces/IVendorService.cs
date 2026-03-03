using BLL.DTOs.VendorDTOs;

namespace BLL.Services.Interfaces
{
    public interface IVendorService
    {
        Task<Result<VendorDetailsDTO>> AddVendorAsync(CreateVendorRequest request);
        Task<Result<VendorDetailsDTO>> DeleteVendorAsync(Guid id);
        Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id);
        Task<Result<List<VendorListDTO>>> GetVendorsAsync();
        Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request);
    }
}