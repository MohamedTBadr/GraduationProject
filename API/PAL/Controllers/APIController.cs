using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PAL.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public abstract class APIController : ControllerBase
    {
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        protected string GetEmailFromToken()
        {
            return User.FindFirstValue(ClaimTypes.Email);
        }
    }
}
