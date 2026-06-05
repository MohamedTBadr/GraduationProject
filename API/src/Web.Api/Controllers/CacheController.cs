using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class CacheController(HybridCache cache) : APIController
    {
        [HttpDelete("invalidate-all")]
        public async Task<IActionResult> InvalidateAllAsync(CancellationToken cancellationToken)
        {
            await cache.RemoveByTagAsync("*", cancellationToken);
            return NoContent();
        }
    }
}
