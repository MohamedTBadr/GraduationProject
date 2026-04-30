using Application;
using Application.DTOs;
using Application.Interfaces;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Web.Api.Attributes;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class VendorTypeController(IVendorTypeService vendorTypeService) : ControllerBase
    {
        private const string CacheKey = "VendorTypes";

        // ─── GET ALL ────────────────────────────────────────────────────────────────

        [HttpGet]
        [HybridCache(1800, CacheKey)]
        [ProducesResponseType(typeof(Result<IReadOnlyList<VendorTypeDetailsDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await vendorTypeService.GetVendorTypesAsync(cancellationToken);
            return result.IsSuccess
                ? Ok(result)
                : result.ToActionResult();
        }

        // ─── GET BY ID ───────────────────────────────────────────────────────────────

        [HttpGet("{id:guid}")]
        [HybridCache(1800, CacheKey)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await vendorTypeService.GetVendorTypeByIdAsync(id, cancellationToken);
            return result.IsSuccess
                ? Ok(result)
                : result.ToActionResult();
        }

        // ─── CREATE ──────────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [InvalidateCache(CacheKey)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrUpdateVendorTypeRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await vendorTypeService.AddVendorTypeAsync(request, cancellationToken);
            return result.IsSuccess ? Created() : result.ToActionResult();
        }

        // ─── UPDATE ──────────────────────────────────────────────────────────────────

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [InvalidateCache(CacheKey)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<VendorTypeDetailsDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] CreateOrUpdateVendorTypeRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await vendorTypeService.UpdateVendorTypeAsync(id, request, cancellationToken);
            return result.IsSuccess
                ? Ok(result)
                : result.ToActionResult();
        }

        // ─── DELETE ──────────────────────────────────────────────────────────────────

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [InvalidateCache(CacheKey)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await vendorTypeService.DeleteVendorTypeAsync(id, cancellationToken);
            return result.IsSuccess
                ? Ok(result)
                : result.ToActionResult();
        }
    }
}