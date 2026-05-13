using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController(IVendorService vendorService, IServiceService serviceService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost("rebuild")]
        public async Task<IActionResult> RebuildIndex()
        {
            // Full rebuild logic
            await vendorService.RebuildSearchIndexAsync();
            await serviceService.RebuildSearchIndexAsync();
            return Ok(new { message = "Search index rebuild started successfully." });
        }
    }
}
