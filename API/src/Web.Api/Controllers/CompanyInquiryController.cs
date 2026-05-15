using Application.DTOs.CompanyInquiryDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using IdempotentAPI.Filters;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyInquiryController : ControllerBase
    {
        private readonly ICompanyInquiryService _service;

        public CompanyInquiryController(ICompanyInquiryService service)
        {
            _service = service;
        }

        // PUBLIC - no auth required (corporations apply without login)
        [HttpPost]
        [Idempotent]
        [AllowAnonymous]
        public async Task<IActionResult> Submit([FromBody] CreateCompanyInquiryDto dto,CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.AddAsync(dto, cancellationToken);
            return Ok(new { message = "Your inquiry has been submitted. We will contact you soon." });
        }

        // ADMIN ONLY below this point
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(request, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        [Idempotent]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyInquiryDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in body." });

            await _service.UpdateAsync(dto, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        [Idempotent]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}