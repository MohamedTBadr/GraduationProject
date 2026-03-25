using Application.DTOs.ServiceTypesDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController(IServiceTypeService ServiceTypeService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllServiceTypes()
        {
            var ServiceTypes = await ServiceTypeService.GetAllServiceTypesAsync();
            return Ok(ServiceTypes);
        }



        [HttpGet("{id}")]
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
        public async Task<IActionResult> AddServiceType(CreateServiceTypeRequest  type)
        {
            await ServiceTypeService.AddTypeAsync(type);
            return Created();
        }
        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceType(Guid id)
        {
            await ServiceTypeService.DeleteTypeAsync(id);
            return NoContent();
        }
        [Authorize(Roles = "Admin")]

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateServiceType(Guid id, UpdateServiceTypeRequest type)
        {
            await ServiceTypeService.UpdateTypeAsync(id, type);
            return Ok();
        }
    }
}
