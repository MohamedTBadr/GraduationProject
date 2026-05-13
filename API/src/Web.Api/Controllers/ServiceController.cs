using Application;
using Application.DTOs.ServiceDTOs;
using Application.Interfaces;
using BLL;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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
    public class ServiceController(IServiceManager serviceManager) : BaseController
    {
        private Guid? UserId =>TryGetUserId();
        private bool IsAdmin => IsAdmin();
        private bool IsVendor => IsVendor();

        // GET api/Services
        [HttpGet]
        [HybridCache(1800, "services")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceService.GetAllAsync(request, IsAdmin, IsVendor, UserId, cancellationToken);
            return  result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        // GET api/Services/{id}
        [HttpGet("{id:guid}")]
        [HybridCache(1800, "services", "services/{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceService.GetByIdAsync(id, cancellationToken);
            return  result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        // GET api/Services/by-event-type/{eventTypeId}
        [HttpGet("by-event-type/{eventTypeId:guid}")]
        [HybridCache(1800, "services")]
        public async Task<IActionResult> GetByEventTypeAsync(Guid eventTypeId, [FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceService.GetByEventTypeIdAsync(eventTypeId, request, IsAdmin, IsVendor, UserId, cancellationToken);
            return Ok(result);
        }

        // GET api/Services/by-vendor/{vendorId}
        [HttpGet("by-vendor/{vendorId:guid}")]
        [HybridCache(1800)]
        public async Task<IActionResult> GetByVendorAsync(Guid vendorId, [FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var filteredRequest = request with { VendorId = vendorId };
            var result = await serviceManager.ServiceService.GetAllAsync(filteredRequest, IsAdmin, IsVendor, UserId, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        // GET api/Services/by-service-type/{serviceTypeId}
        [HttpGet("by-service-type/{serviceTypeId:guid}")]
        [HybridCache(1800)]
        public async Task<IActionResult> GetByServiceTypeAsync(Guid serviceTypeId, [FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var filteredRequest = request with { ServiceTypeId = serviceTypeId };
            var result = await serviceManager.ServiceService.GetAllAsync(filteredRequest, IsAdmin, IsVendor, UserId, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }
        // POST api/Services
        [Authorize(Roles = "Vendor")]
        [HttpPost]
        [Idempotent]
        [InvalidateCache("services")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateServiceRequest dto, CancellationToken cancellationToken)
        {
            dto.VendorId = UserId; // Ensure the Service is associated with the authenticated vendor
            var result = await serviceManager.ServiceService.CreateAsync(dto, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result); // filter handles the failure

            return  result.IsSuccess ? Created() : result.ToActionResult();
        }

        // PUT api/Services/{id}

        [Authorize(Roles = "Admin,Vendor")]
        [HttpPut("{id:guid}")]
        [Idempotent]
        [InvalidateCache("services/{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] UpdateServiceDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.Id)
                return BadRequest(Error.Validation(422, "Route id and body id do not match."));

            if (!IsAdmin) // Vendor: verify ownership and force their own VendorId
            {
                var service = await serviceManager.ServiceService.GetByIdAsync(id, cancellationToken);

                if (service.Value.VendorId != UserId)
                    return Forbid();

                dto.VendorId = UserId; // Vendor can only assign themselves
            }
            // Admin: dto.VendorId is used as-is (they can assign any vendor)

            var result = await serviceManager.ServiceService.UpdateAsync(dto, cancellationToken);

            return  result.IsSuccess ? NoContent() : result.ToActionResult();
        }
        [Authorize(Roles = "Admin,Vendor")]

        // DELETE api/Services/{id}
        [HttpDelete("{id:guid}")]
        [Idempotent]
        [InvalidateCache("services/{id}", "services")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            if (UserId != null && !IsAdmin) // if user is authenticated and not admin, ensure they own the service
            {
                var service = await serviceManager.ServiceService.GetByIdAsync(id, cancellationToken);
                if (service.Value.VendorId != UserId)
                    return Forbid(); // filter handles the failure
            }
            var result = await serviceManager.ServiceService.DeleteAsync(id, cancellationToken);
            if (result.IsSuccess)
                return NoContent();

            return  result.IsSuccess ? NoContent() : result.ToActionResult();
        }
    
        [HttpPatch("{id}/status")]
        [Idempotent]
        [Authorize(Roles = "Admin,Vendor")]
        [InvalidateCache("services/{id}","services")]
        public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken ct)
        {
            await serviceManager.ServiceService.ToggleStatusAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id}/ratings")]
        [Idempotent]
        [Authorize(Roles = "User")]
        [InvalidateCache("services/{id}")]
        public async Task<IActionResult> AddRatingAsync(Guid id, ServiceRatingRequest dto, CancellationToken cancellationToken)
        {
            dto.UserId = UserId; // Ensure the rating is associated with the authenticated user
            dto.ServiceId = id; // Ensure the rating is associated with the correct service
            await serviceManager.ServiceService.AddRatingAsync(dto, cancellationToken);
            
            return Created(); // filter handles the failure
        }
    }
}