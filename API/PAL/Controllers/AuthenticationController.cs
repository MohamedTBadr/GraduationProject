using BLL.DTOs.AuthenticationDTOs;
using BLL.Services.Interfaces;
using Common.Exceptions;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IAuthenticationService authenticationService):APIController
    {
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {

                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();



                throw new BadRequestException(errors);
            }

            return Ok(await authenticationService.LogIn(loginRequest));
        }

        [HttpPost("Register")]
        [Idempotent]
        public async Task<IActionResult> Register(SignUpRequest request)
        {
            if (!ModelState.IsValid)
            {
              
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                

                throw new BadRequestException(errors);
            }

            return Ok(await authenticationService.RegisterAsync(request));
        }

        [HttpPost("CheckIfEmailExists")]
        public async Task<IActionResult> CheckEmailExists([Required][FromQuery] string email)
        {
            if (!ModelState.IsValid)
            {

                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();



                throw new BadRequestException(errors);
            }

            return Ok(await authenticationService.CheckIfEmailExists(email));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var email = GetEmailFromToken();
            return Ok(await authenticationService.GenerateRefreshToken(email));
        }
    }
}
