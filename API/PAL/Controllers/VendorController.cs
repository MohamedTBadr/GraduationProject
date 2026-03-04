using BLL;
using BLL.DTOs.VendorDTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PAL.Controllers.Attributes;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IVendorService vendorService) : ControllerBase
    {
        [HttpGet]
        public async Task<Result<List<VendorListDTO>>> GetVendorsAsync()
        {
            return await vendorService.GetVendorsAsync();
        }

        [HttpGet("{id}")]
        public async Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id)
        {
            return await vendorService.GetVendorByIdAsync(id);
        }

        [HttpPost]
        [SuccessStatusCode(201)]
        public async Task<Result<VendorDetailsDTO>> CreateVendorAsync(CreateVendorRequest request)
        {
            return await vendorService.AddVendorAsync(request);
        }

        [HttpDelete("{id}")]
        [SuccessStatusCode(204)]

        public async Task<Result<VendorDetailsDTO>> DeleteVendorAsync(Guid id)
        {
            return await vendorService.DeleteVendorAsync(id);
        }

        [HttpPatch("{id}")]
        public async Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request)
        {
            return await vendorService.UpdateVendorAsync(id, request);
        }
    }
}
