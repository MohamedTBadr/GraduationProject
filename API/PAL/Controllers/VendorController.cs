using BLL;
using BLL.DTOs.VendorDTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IVendorService vendorService) : ControllerBase
    {
        [HttpGet]
        public async Task<Result<List<VendorListDTO>>> GetVendorsAsync()
        {
            var vendorsResult = await vendorService.GetVendorsAsync(); // Result<List<VendorListDTO>>

            if (!vendorsResult.IsSuccess)
                return Result<List<VendorListDTO>>.Failure(vendorsResult.ErrorType, vendorsResult.ErrorMessage);

            return Result<List<VendorListDTO>>.Success(vendorsResult.Value);
        }


        [HttpGet("{id}")]
        public async Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id)
        {
            var vendorResult = await vendorService.GetVendorByIdAsync(id);

            if (!vendorResult.IsSuccess)
                return Result<VendorDetailsDTO>.Failure(vendorResult.ErrorType, vendorResult.ErrorMessage);

            return Result<VendorDetailsDTO>.Success(vendorResult.Value);
        }

        [HttpPost]
        public async Task<Result<VendorDetailsDTO>> CreateVendorAsync(CreateVendorRequest request)
        {
            var createResult = await vendorService.AddVendorAsync(request);
            if (!createResult.IsSuccess)
                return Result<VendorDetailsDTO>.Failure(createResult.ErrorType, createResult.ErrorMessage);
            return Result<VendorDetailsDTO>.Success(createResult.Value);
        }
        [HttpDelete]
        public async Task<Result<bool>> DeleteVendorAsync(Guid id)
        {
            var deleteResult = await vendorService.DeleteVendorAsync(id);
            if (!deleteResult.IsSuccess)
                return Result<bool>.Failure(deleteResult.ErrorType, deleteResult.ErrorMessage);
            return Result<bool>.Success(true);
        }
        [HttpPatch("{id}")]
        public async Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request)
        {
            var updateResult = await vendorService.UpdateVendorAsync(id, request);
            if (!updateResult.IsSuccess)
                return Result<VendorDetailsDTO>.Failure(updateResult.ErrorType, updateResult.ErrorMessage);
            return Result<VendorDetailsDTO>.Success(updateResult.Value);
        }
    }
}
