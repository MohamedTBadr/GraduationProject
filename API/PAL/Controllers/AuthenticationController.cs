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
    public class AuthenticationController(IServiceManager serviceManager) : APIController
    {
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {

                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();



                throw new UnprocessableContentException(errors);
            }

            return Ok(await serviceManager.AuthenticationService.LogIn(loginRequest));
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

            return Ok(await serviceManager.AuthenticationService.RegisterAsync(request));
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

            return Ok(await serviceManager.AuthenticationService.CheckIfEmailExists(email));
        }

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        [HttpPost("RefreshToken")]
        [ProducesResponseType(200, Type = typeof(UserResponse))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            try
            {
                // Call the secure service method with the token provided by the client
                var response = await serviceManager.AuthenticationService.RefreshTokenAsync(request);

                // The UserResponse now contains the new AccessToken and the new RefreshToken
                return Ok(response);
            }
            catch (UnauthorizedException ex)
            {
                // Handles cases where the refresh token is invalid, expired, or not found.
                return Unauthorized(new { message = ex.Message });
            }
        }


        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromQuery][Required] string email)
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
            await serviceManager.AuthenticationService.ForgetPassword(email);
            return Ok();


        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
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
            await serviceManager.AuthenticationService.ResetPassword(request);
            return Ok();
        }

    }
}
