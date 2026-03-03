using BLL.DTOs.UserDTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<ApplicationUser> userManager):APIController
    {
        [HttpGet]
        public async Task<List<UserDTO>> GetAllUsers()
        {
            var users = await userManager.Users.ToListAsync();

            var userDtos = users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.UserName,
                Email = u.Email
            }).ToList();

            return userDtos;
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
        public async Task<IActionResult> CreateUser([FromBody] UserDTO userDto)
        {

            var user = new ApplicationUser
            {
                UserName = userDto.Name,
                Email = userDto.Email
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
        }




    }
}