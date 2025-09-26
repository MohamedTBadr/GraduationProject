using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PAL.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public abstract class APIController : ControllerBase
    {
        protected string GetEmailFromToken() => User.FindFirstValue(ClaimTypes.Email);
    }
}
