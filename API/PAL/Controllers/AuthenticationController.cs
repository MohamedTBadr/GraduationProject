using BLL.DTOs.AuthenticationDTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IAuthenticationService authenticationService):APIController
    {
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest) => Ok(await authenticationService.LogIn(loginRequest));

        [HttpPost("Register")]
        public async Task<IActionResult> Register(SignUpRequest request) => Ok(await authenticationService.RegisterAsync(request));


        [HttpPost("CheckIfEmailExists")]
        public async Task<IActionResult> CheckEmailExists([FromQuery]string email) => Ok(await authenticationService.CheckIfEmailExists(email));

        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            return Ok(await authenticationService.GenerateRefreshToken(email));
        }
    }
}
