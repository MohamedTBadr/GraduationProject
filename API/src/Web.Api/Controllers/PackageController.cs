using Application;
using Application.DTOs.PackageDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Threading;
using System.Threading.Tasks;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackageController(IServiceManager serviceManager) : APIController
    {
        private Guid? UserId => TryGetUserId();
        private bool IsAdminUser => IsAdmin();
        private bool IsVendorUser => IsVendor();

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await serviceManager.PackageService.GetAllAsync(request, IsAdminUser, IsVendorUser, UserId, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await serviceManager.PackageService.GetByIdAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [Authorize(Roles = "Vendor,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePackageDTO dto, CancellationToken cancellationToken)
        {
            if (IsVendorUser)
            {
                dto.VendorId = UserId.Value; // Ensure the Package is associated with the authenticated vendor
            }

            var result = await serviceManager.PackageService.CreateAsync(dto, cancellationToken);

            return result.IsSuccess ? Created("", result) : result.ToActionResult();
        }

        [Authorize(Roles = "Admin,Vendor")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdatePackageDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.Id)
                return BadRequest(Error.Validation(422, "Route id and body id do not match."));

            if (IsVendorUser)
            {
                var package = await serviceManager.PackageService.GetByIdAsync(id, cancellationToken);
                if (package.Value?.VendorId != UserId.Value)
                    return Forbid();

                dto.VendorId = UserId.Value;
            }

            var result = await serviceManager.PackageService.UpdateAsync(dto, cancellationToken);

            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }

        [Authorize(Roles = "Admin,Vendor")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            if (IsVendorUser)
            {
                var package = await serviceManager.PackageService.GetByIdAsync(id, cancellationToken);
                if (package.Value?.VendorId != UserId.Value)
                    return Forbid();
            }

            var result = await serviceManager.PackageService.DeleteAsync(id, cancellationToken);

            return result.IsSuccess ? NoContent() : result.ToActionResult();
        }
    }
}
