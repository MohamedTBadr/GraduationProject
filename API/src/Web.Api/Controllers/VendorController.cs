using Application;
using Application.DTOs.VendorDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IVendorService vendorService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetVendorsAsync()
        {
            var vendors= await vendorService.GetVendorsAsync();
            return Ok(vendors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorByIdAsync(Guid id)
        {
           var vendor= await vendorService.GetVendorByIdAsync(id);
            return Ok(vendor);
        }

        [HttpPost]
        [SuccessStatusCode(201)]
        public async Task<IActionResult> CreateVendorAsync(CreateVendorRequest request)
        {
             await vendorService.AddVendorAsync(request);
            return Created();
        }




        [HttpDelete("{id}")]
        [SuccessStatusCode(204)]

        public async Task<IActionResult> DeleteVendorAsync(Guid id)
        {
          var result= await vendorService.DeleteVendorAsync(id);
            return NoContent();
        }
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateVendorAsync(Guid id, UpdateVendorRequest request)
        {
             await vendorService.UpdateVendorAsync(id, request);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> ApproveVendorAsync(Guid id)
        {
             await vendorService.ApproveVendorAsync(id);
            return NoContent();
        }
    }
}
