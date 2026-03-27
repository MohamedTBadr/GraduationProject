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
        public async Task<IActionResult> GetAllServiceTypes()
        {
            var ServiceTypes = await ServiceTypeService.GetAllServiceTypesAsync();
            return Ok(ServiceTypes);
        }



        [HttpGet("{id}")]
        [HybridCache(1800, "serviceTypes/{id}" )]

        public async Task<IActionResult> GetServiceTypeById(Guid id)
        {
            var ServiceType = await ServiceTypeService.GetServiceTypeByIdAsync(id);
            if (ServiceType == null)
            {
                return NotFound();
            }
            return Ok(ServiceType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [InvalidateCache("serviceTypes")]
        public async Task<IActionResult> AddServiceType(CreateServiceTypeRequest  type)
        {
            await ServiceTypeService.AddTypeAsync(type);
            return Created();
        }
        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        [InvalidateCache("serviceTypes/{id}", "serviceTypes")]
        public async Task<IActionResult> DeleteServiceType(Guid id)
        {
            await ServiceTypeService.DeleteTypeAsync(id);
            return NoContent();
        }
        [Authorize(Roles = "Admin")]

        [HttpPatch("{id}")]
        [InvalidateCache("serviceTypes/{id}", "serviceTypes")]
        public async Task<IActionResult> UpdateServiceType(Guid id, UpdateServiceTypeRequest type)
        {
            await ServiceTypeService.UpdateTypeAsync(id, type);
            return Ok();
        }
    }
}
