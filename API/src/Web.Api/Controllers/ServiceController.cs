using Application;
using Application.DTOs.ServiceDTOs;
using Application.Interfaces;
using BLL;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController(IServiceService ServiceService) : BaseController
    {
        private Guid userId => GetUserIdFromToken();
        // GET api/Services
        [HttpGet]
        [HybridCache(1800, "services")]

        public async Task<IActionResult> GetAllAsync([FromQuery] PaginatedRequest request)
        {
            var result = await ServiceService.GetAllAsync(request);
            return Ok(result);
        }

        // GET api/Services/{id}
        [HttpGet("{id:guid}")]
        [HybridCache(1800, "services", "services/{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await ServiceService.GetByIdAsync(id);
            return Ok(result);
        }

        // GET api/Services/by-category/{categoryId}
        [HttpGet("by-category/{categoryId:guid}")]
        [HybridCache(1800, "services")]

        public async Task<IActionResult> GetByCategoryAsync(Guid categoryId, [FromQuery] PaginatedRequest request)
        {
            var result = await ServiceService.GetByCategoryIdAsync(categoryId, request);
            return Ok(result);
        }

        // GET api/Services/by-vendor/{vendorId}
        [HttpGet("by-vendor/{vendorId:guid}")]
        [HybridCache(1800)]

        public async Task<IActionResult> GetByVendorAsync(Guid vendorId, [FromQuery] PaginatedRequest request)
        {
            var result = await ServiceService.GetByVendorIdAsync(vendorId, request);
            return Ok(result);
        }

        // GET api/Services/by-Service-type/{ServiceTypeId}
        [HttpGet("by-Service-type/{ServiceTypeId:guid}")]
        [HybridCache(1800)]

        public async Task<IActionResult> GetByServiceTypeAsync(Guid ServiceTypeId, [FromQuery] PaginatedRequest request)
        {
            var result = await ServiceService.GetByServiceTypeIdAsync(ServiceTypeId, request);
            return Ok(result);
        }

        // POST api/Services
        [Authorize(Roles = "Vendor")]
        [HttpPost]
        [InvalidateCache("services")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateServiceRequest dto)
        {
            dto.VendorId = userId; // Ensure the Service is associated with the authenticated vendor
            var result = await ServiceService.CreateAsync(dto);

            if (result.IsFailure)
                return BadRequest(result); // filter handles the failure

            return Created(); // filter handles the failure
        }
        [Authorize(Roles = "Admin")]

        // PUT api/Services/{id}
        [HttpPut("{id:guid}")]
        [InvalidateCache("services/{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateServiceDTO dto)
        {
            if (id != dto.Id)
                return BadRequest(Error.Validation("Service.IdMismatch", "Route id and body id do not match."));

            var result = await ServiceService.UpdateAsync(dto);
      
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]

        // DELETE api/Services/{id}
        [HttpDelete("{id:guid}")]
        [InvalidateCache("services/{id}", "services")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await ServiceService.DeleteAsync(id);

            if (result.IsSuccess)
                return NoContent();

            return Ok(result); // filter handles the failure
        }
    }
}