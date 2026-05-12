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
    public class ServiceTypeController(IServiceManager serviceManager) : ControllerBase
    {
        [HttpGet]
        [HybridCache(1800,"serviceTypes")]
        public async Task<IActionResult> GetAllServiceTypes(CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceTypeService.GetAllServiceTypesAsync(cancellationToken);
            return result.IsSuccess ? Ok(result) : result.ToActionResult();
        }



        [HttpGet("{id}")]
        [HybridCache(1800, "serviceTypes/{id}" )]

        public async Task<IActionResult> GetServiceTypeById(Guid id, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceTypeService.GetServiceTypeByIdAsync(id, cancellationToken);
           
            return  result.IsSuccess ? Ok(result) : result.ToActionResult();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [InvalidateCache("serviceTypes")]
        public async Task<IActionResult> AddServiceType(CreateServiceTypeRequest  type, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceTypeService.AddTypeAsync(type, cancellationToken);
            return  result.IsSuccess ? Created() : result.ToActionResult();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [InvalidateCache("serviceTypes/{id}", "serviceTypes")]
        public async Task<IActionResult> DeleteServiceType(Guid id, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceTypeService.DeleteTypeAsync(id, cancellationToken);
            return  result.IsSuccess ? NoContent() : result.ToActionResult();
        }
        [Authorize(Roles = "Admin")]

        [HttpPatch("{id}")]
        [InvalidateCache("serviceTypes/{id}", "serviceTypes")]
        public async Task<IActionResult> UpdateServiceType(Guid id, UpdateServiceTypeRequest type, CancellationToken cancellationToken)
        {
            var result = await serviceManager.ServiceTypeService.UpdateTypeAsync(id, type, cancellationToken);
            return  result.IsSuccess ? NoContent() : result.ToActionResult();
        }
    }
}
