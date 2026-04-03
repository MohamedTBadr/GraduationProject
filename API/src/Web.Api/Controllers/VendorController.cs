using Application;
using Application.DTOs.VendorDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IVendorService vendorService) : BaseController
    {
        [HttpGet]
        [HybridCache(1800,"vendors")]
        public async Task<IActionResult> GetVendorsAsync(CancellationToken cancellationToken)
        {
            var vendors= await vendorService.GetVendorsAsync(cancellationToken);
            return Ok(vendors);
        }

        [HttpGet("{id}")]
        [HybridCache(1800, "vendors", "vendors/{id}")]
        public async Task<IActionResult> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
           var vendor= await vendorService.GetVendorByIdAsync(id, cancellationToken);
            return Ok(vendor);
        }

        [HttpPost]
        [SuccessStatusCode(201)]
        [ProducesResponseType(400)]
        [InvalidateCache]
        public async Task<IActionResult> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken)
        {
            var result=  await vendorService.AddVendorAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Created();
        }


        [HttpPost("{id}/rating")]
        [Authorize]
        [InvalidateCache]
        public async Task<IActionResult> RateVendorAsync(Guid id, RatingVendorRequest request, CancellationToken cancellationToken)
        {
             await vendorService.RateVendorAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [SuccessStatusCode(204)]
        [InvalidateCache]
        public async Task<IActionResult> DeleteVendorAsync(Guid id, CancellationToken cancellationToken)
        {
          var result= await vendorService.DeleteVendorAsync(id, cancellationToken);
            return NoContent();
        }
        [Authorize]
        [HttpPatch("{id}")]
        [InvalidateCache]
        public async Task<IActionResult> UpdateVendorAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken)
        {
             await vendorService.UpdateVendorAsync(id, request, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/approve")]
        [InvalidateCache]
        public async Task<IActionResult> ApproveVendorAsync(Guid id, CancellationToken cancellationToken)
        {
             await vendorService.ApproveVendorAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
