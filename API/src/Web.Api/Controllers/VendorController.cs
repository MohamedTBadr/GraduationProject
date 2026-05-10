using Application;
using Application.DTOs.VendorDTOs;
using Application.Interfaces;
using Application.Services;
using IdempotentAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System.Security.Claims;
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
        public async Task<IActionResult> GetVendorsAsync([FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
        {
            var isAdmin = User.IsInRole("Admin");

            var result = await vendorService.GetVendorsAsync(paginatedRequest,isAdmin, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
        }

        [HttpGet("{id}")]
        [HybridCache(1800, "vendors", "vendors/{id}")]
        public async Task<IActionResult> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
           var result= await vendorService.GetVendorByIdAsync(id, cancellationToken);
            return result.IsSuccess? Ok(result.Value) : result.ToActionResult();
        }


        /// <summary>
        /// Get all bookings for the authenticated vendor.
        /// GET /api/vendor/bookings
        /// </summary>
        /// 
        [HttpGet("bookings")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
        {
            var vendorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(vendorIdClaim) || !Guid.TryParse(vendorIdClaim, out var vendorId))
                return Unauthorized(new { message = "Invalid or missing vendor identity." });

            var bookings = await vendorService.GetVendorBookingsAsync(vendorId, cancellationToken);

            if (!bookings.Any())
                return NotFound(new { message = "No bookings found for this vendor." });

            return Ok(bookings);
        }

        [HttpPost]
        [SuccessStatusCode(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [InvalidateCache]
        public async Task<IActionResult> CreateVendorAsync([FromForm]CreateVendorRequest request, CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest();
            var result=  await vendorService.AddVendorAsync(request, cancellationToken);
            return result.IsSuccess ? Created() : result.ToActionResult();
        }


        [HttpPost("{id}/rating")]
        [Authorize]
        [InvalidateCache]
        public async Task<IActionResult> RateVendorAsync(Guid id, RatingVendorRequest request, CancellationToken cancellationToken)
        {
            var result= await vendorService.RateVendorAsync(id, request, cancellationToken);
            return result.IsSuccess? NoContent():result.ToActionResult();
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
            var result = await vendorService.UpdateVendorAsync(id, request, cancellationToken);
            return result.IsSuccess? NoContent(): result.ToActionResult();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/approve")]
        [InvalidateCache]
        public async Task<IActionResult> ApproveVendorAsync(Guid id, CancellationToken cancellationToken)
        {
            var result=  await vendorService.ApproveVendorAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }
    }
}
