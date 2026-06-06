using Application;
using Application.DTOs.UserDTOs;
using Application.Interfaces;
using Application.Services.Helpers;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using Web.Api.Attributes;
using Web.Api.Controllers.Attributes;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<ApplicationUser> userManager, IEmailSender emailSenderService): APIController
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [HybridCache(1800, "GetAllUsers", Variance = CacheVariance.Adaptive)]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginatedRequest request, CancellationToken cancellationToken)
        {
            var query = userManager.Users.AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.UserName.Contains(request.SearchTerm) ||
                    u.Email.Contains(request.SearchTerm));
            }

            // 📊 Total count BEFORE pagination
            var totalCount = await query.CountAsync();

            // 📄 Pagination
            var users = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var userDtos = users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.UserName,
                Email = u.Email
            }).ToList();


            var response = Result<PaginatedResponse<UserDTO>>.Success(new PaginatedResponse<UserDTO>(userDtos,totalCount,request.PageIndex,request.PageSize));

            return response.IsSuccess ? Ok(response) : response.ToActionResult();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [InvalidateCache("GetAllUsers")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await userManager.DeleteAsync(user );
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }


        [HttpGet("{id}")]
        [Authorize]
        [HybridCache(1800, "GetUserById", Variance = CacheVariance.Adaptive)]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
            if (!IsAdminOrOwner(id))
                return Forbid();

            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDTO
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email
            };

            return Ok(userDto);
        }

        [HttpPatch("suspend/{id}")]
        [Authorize(Roles = "Admin")]
        [InvalidateCache("GetUserById", "GetAllUsers")]
        public async Task<IActionResult> SuspendUser(Guid id, [FromBody] string reason)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.IsSuspended = true;
            user.SuspensionReason = reason;

            // We still set LockoutEnd so the built-in Login system blocks them automatically
            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(7);
            var until = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);
            await emailSenderService.SendAccountSuspensionEmailAsync(user.Email, user.UserName, reason, until);
            return NoContent();
        }


        [HttpPatch("unsuspend/{id}")]
        [Authorize(Roles = "Admin")]
        [InvalidateCache("GetUserById", "GetAllUsers")]
        public async Task<IActionResult> UnsuspendUser(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // 1. Reset the business flags
            user.IsSuspended = false;
            user.SuspensionReason = null;

            // 2. Clear the technical lockout so the login system allows them in
            // Passing 'null' removes the restriction immediately
            await userManager.SetLockoutEndDateAsync(user, null);

            // 3. Optional: Reset failed attempts 
            // This prevents them from being locked out again if they had failed 
            // logins before the admin suspended them.
            await userManager.ResetAccessFailedCountAsync(user);

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await emailSenderService.SendEmailAsync(user.Email, "Account Unsuspended", "Your account has been unsuspended. You can now log in.");
            return NoContent();
        }






        [HttpPatch("{id}")]
        [Authorize]
        [InvalidateCache("GetUserById", "GetAllUsers")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserDTO userDto)
        {
            if (!IsAdminOrOwner(id))
                return Forbid();

            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = userDto.Name;
            user.Email = userDto.Email;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
        [Authorize(Roles = "Admin")]

        [HttpPost]
        [SuccessStatusCode(201)]
        [HybridCache(1800, "GetAllUsers", Variance = CacheVariance.Adaptive)]
        public async Task<Result<UserDTO>> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Name,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var (errorType, message) = ToError(result);

                var error = errorType switch
                {
                    ErrorType.Validation => Error.Validation(400, message),
                    ErrorType.Conflict => Error.Conflict(409, message),
                    ErrorType.Unauthorized => Error.Unauthorized(401, message),
                    _ => Error.Unexpected(500, message)
                };

                return Result<UserDTO>.Failure(error);
            }
            return Result<UserDTO>.Success(new UserDTO
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email
            });
        }

        private static (ErrorType, string) ToError(IdentityResult result)
        {
            var description = string.Join("; ", result.Errors.Select(e => e.Description));
            return (ErrorType.Validation, description);
        }

    }
}
