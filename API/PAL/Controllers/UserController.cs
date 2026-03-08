using API.Controllers;
using BLL;
using BLL.DTOs.UserDTOs;
using Common;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAL.Controllers.Attributes;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<ApplicationUser> userManager): BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginatedRequest request)
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

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserDTO userDto)
        {
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

        [HttpPost]
        [SuccessStatusCode(201)]
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
                return Result<UserDTO>.Failure(errorType, message);
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