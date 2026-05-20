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
using IdempotentAPI.Filters;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController(IServiceManager serviceManager) : APIController
    {
        [HttpGet]
        [HybridCache(1800,"vendors", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetVendorsAsync([FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
        {
            var isAdmin = User.IsInRole("Admin");

            var result = await serviceManager.VendorService.GetVendorsAsync(paginatedRequest,isAdmin, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [HttpGet("{id}")]
        [HybridCache(1800, "vendors", "vendors/{id}")]
        public async Task<IActionResult> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
           var result= await serviceManager.VendorService.GetVendorByIdAsync(id, cancellationToken);
            return result.IsSuccess? Ok(result.Value) : result.ToActionResult();
        }


        /// <summary>
        /// Get all bookings for the authenticated vendor.
        /// GET /api/vendor/bookings
        /// </summary>
        /// 
        [HttpGet("bookings")]
        [Authorize(Roles = "Vendor")]
        [HybridCache(1800, "vendors", "vendors/{UserId}/bookings", Variance = CacheVariance.PerUser)]
        public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
        {
            var vendorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(vendorIdClaim) || !Guid.TryParse(vendorIdClaim, out var vendorId))
                return Unauthorized(new { message = "Invalid or missing vendor identity." });

            var bookings = await serviceManager.VendorService.GetVendorBookingsAsync(vendorId, cancellationToken);

            if (!bookings.Any())
                return NotFound(new { message = "No bookings found for this vendor." });

            return Ok(bookings);
        }

        [HttpPost]
        [Idempotent]
        [SuccessStatusCode(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [InvalidateCache("vendors")]
        public async Task<IActionResult> CreateVendorAsync([FromForm]CreateVendorRequest request, CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest();
            var result=  await serviceManager.VendorService.AddVendorAsync(request, cancellationToken);
            return result.IsSuccess ? Created() : result.ToActionResult();
        }


        [HttpPost("{id}/rating")]
        [Idempotent]
        [Authorize]
        [InvalidateCache("vendors", "vendors/{id}")]
        public async Task<IActionResult> RateVendorAsync(Guid id, RatingVendorRequest request, CancellationToken cancellationToken)
        {
            var result= await serviceManager.VendorService.RateVendorAsync(id, request, cancellationToken);
            return result.IsSuccess? NoContent():result.ToActionResult();
        }

        [HttpDelete("{id}")]
        [Idempotent]
        [Authorize(Roles = "Admin")]
        [SuccessStatusCode(204)]
        [InvalidateCache("vendors", "vendors/{id}")]
        public async Task<IActionResult> DeleteVendorAsync(Guid id, CancellationToken cancellationToken)
        {
          var result= await serviceManager.VendorService.DeleteVendorAsync(id, cancellationToken);
            return NoContent();
        }
        [Authorize(Roles = "Vendor")]
        [HttpPatch("{id}")]
        [Idempotent]
        [InvalidateCache("vendors", "vendors/{id}", "vendors/{id}/bookings")]
        public async Task<IActionResult> UpdateVendorAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken)
        {
            if (!IsAdminOrOwner(id))
                return Forbid();

            var result = await serviceManager.VendorService.UpdateVendorAsync(id, request, cancellationToken);
            return result.IsSuccess? NoContent(): result.ToActionResult();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/approve")]
        [Idempotent]
        [InvalidateCache("vendors", "vendors/{id}")]
        public async Task<IActionResult> ApproveVendorAsync(Guid id, CancellationToken cancellationToken)
        {
            var result=  await serviceManager.VendorService.ApproveVendorAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }

        [HttpGet("{id}/vibe")]
        [HybridCache(3600, "vendors", "vendors/{id}/vibe")]
        public async Task<IActionResult> GetVendorVibeAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await serviceManager.VendorService.GetVendorVibeAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }
    }
}
