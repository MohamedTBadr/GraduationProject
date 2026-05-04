using Application;
using Application.DTOs.AuthenticationDTOs;
using Application.Interfaces;
using Domain.Entities;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IServiceManager ServiceManager) : BaseController
    {
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken)
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
            var user = await ServiceManager.AuthenticationService.LogIn(loginRequest, cancellationToken);

            var response = Result<UserResponse>.Success(user);
            return Ok(response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(SignUpRequest request,CancellationToken cancellationToken)
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
            var result = await ServiceManager.AuthenticationService.RegisterAsync(request, cancellationToken);
            var response = Result<UserResponse>.Success(result);
            return Ok(response);
        }

        [HttpPost("CheckIfEmailExists")]
        public async Task<IActionResult> CheckEmailExists([Required][FromQuery] string email,CancellationToken cancellationToken)
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

            return Ok(await ServiceManager.AuthenticationService.CheckIfEmailExists(email, cancellationToken));
        }

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        [HttpPost("RefreshToken")]
        [ProducesResponseType(200, Type = typeof(UserResponse))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request,CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            try
            {
                // Call the secure Service method with the token provided by the client
                var response = await ServiceManager.AuthenticationService.RefreshTokenAsync(request, cancellationToken);
                var result = Result<UserResponse>.Success(response);
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
        public async Task<IActionResult> ForgetPassword([FromQuery][Required] string email,CancellationToken cancellationToken)
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
            await ServiceManager.AuthenticationService.ForgetPassword(email, cancellationToken);
            var result = Result<string>.Success("Email Has Send Succussfully");
            return Ok(result);


        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
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
            await ServiceManager.AuthenticationService.ResetPassword(request, cancellationToken);

            var result = Result<string>.Success("Email Has Send Succussfully");

            return Ok(result);
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            await ServiceManager.AuthenticationService.LogoutAsync(Guid.Parse(userId), cancellationToken);
            var result = Result<string>.Success("Logged out successfully");
            return Ok(result);
        }

    }
}
