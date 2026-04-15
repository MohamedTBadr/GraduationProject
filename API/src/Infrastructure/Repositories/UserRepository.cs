using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository(UserManager<ApplicationUser> userManager,
                            ApplicationDbContext dbContext) : IUserRepository
    {
        public async Task<ApplicationUser?> GetByEmailAsync(string email)
            => await userManager.FindByEmailAsync(email);

        public async Task<ApplicationUser?> GetByNameAsync(string name)
            => await userManager.FindByNameAsync(name);

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
            => await userManager.FindByIdAsync(userId);

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken)
            => await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
            => await userManager.GetRolesAsync(user);

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
            => await userManager.CreateAsync(user, password);

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
            => await userManager.UpdateAsync(user);

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
            => await userManager.CheckPasswordAsync(user, password);

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
            => await userManager.GeneratePasswordResetTokenAsync(user);

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
            => await userManager.ResetPasswordAsync(user, token, newPassword);

        public async Task<bool> IsVendorVerifiedAsync(Guid userId)
        {
            var vendor = await dbContext.Vendors.FirstOrDefaultAsync(v => v.UserId == userId);
            return vendor?.IsVerified ?? false;
        }
    }
}
