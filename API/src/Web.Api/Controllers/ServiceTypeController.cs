using Application.DTOs.ServiceTypesDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController(IServiceTypeService ServiceTypeService) : ControllerBase
    {
        [HttpGet]
        [HybridCache(1800,"serviceTypes")]
        public async Task<IActionResult> GetAllServiceTypes(CancellationToken cancellationToken)
        {
            var ServiceTypes = await ServiceTypeService.GetAllServiceTypesAsync(cancellationToken);
            return Ok(ServiceTypes);
        }



        [HttpGet("{id}")]
        [HybridCache(1800, "serviceTypes/{id}" )]

        public async Task<IActionResult> GetServiceTypeById(Guid id, CancellationToken cancellationToken)
        {
            var ServiceType = await ServiceTypeService.GetServiceTypeByIdAsync(id, cancellationToken);
            if (ServiceType == null)
            {
                return NotFound();
            }
            return Ok(ServiceType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [InvalidateCache("serviceTypes")]
        public async Task<IActionResult> AddServiceType(CreateServiceTypeRequest  type, CancellationToken cancellationToken)
        {
            await ServiceTypeService.AddTypeAsync(type, cancellationToken);
            return Created();
        }
        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        [InvalidateCache("serviceTypes/{id}", "serviceTypes")]
        public async Task<IActionResult> DeleteServiceType(Guid id, CancellationToken cancellationToken)
        {
            await ServiceTypeService.DeleteTypeAsync(id, cancellationToken);
            return NoContent();
        }
        [Authorize(Roles = "Admin")]

        [HttpPatch("{id}")]
        [InvalidateCache("serviceTypes/{id}", "serviceTypes")]
        public async Task<IActionResult> UpdateServiceType(Guid id, UpdateServiceTypeRequest type, CancellationToken cancellationToken)
        {
            await ServiceTypeService.UpdateTypeAsync(id, type, cancellationToken);
            return Ok();
        }
    }
}
