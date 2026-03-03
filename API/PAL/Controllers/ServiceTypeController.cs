using BLL.DTOs.ServiceTypesDTOs;
using BLL.Services;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController(ServiceTypeService serviceTypeService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllServiceTypes()
        {
            var serviceTypes = await serviceTypeService.GetAllServiceTypesAsync();
            return Ok(serviceTypes);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceTypeById(Guid id)
        {
            var serviceType = await serviceTypeService.GetServiceTypeByIdAsync(id);
            if (serviceType == null)
            {
                return NotFound();
            }
            return Ok(serviceType);
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> AddServiceType(CreateServiceTypeRequest  type)
        {
            await serviceTypeService.AddTypeAsync(type);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceType(Guid id)
        {
            await serviceTypeService.DeleteTypeAsync(id);
            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateServiceType(Guid id, UpdateServiceTypeRequest type)
        {
            await serviceTypeService.UpdateTypeAsync(id, type);
            return Ok();
        }
    }
}
