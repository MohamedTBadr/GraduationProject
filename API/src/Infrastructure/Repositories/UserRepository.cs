using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class UserRepository(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext) : IUserRepository
    {

        // Identity UserManager doesn't always accept tokens for 'Find' methods, 
        // but we pass 'cancellationToken' to Polly to allow the pipeline to cancel retries.

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await userManager.FindByEmailAsync(email);

        public async Task<ApplicationUser?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => await userManager.FindByNameAsync(name);

        public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await userManager.FindByIdAsync(userId.ToString());

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => 
                await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken,cancellationToken);


        public async Task<ApplicationUser?> GetByReferralCodeAsync(string referralCode, CancellationToken ct) =>
    await dbContext.Users
            .FirstOrDefaultAsync(u => u.ReferralCode == referralCode, ct);

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => await userManager.GetRolesAsync(user);

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password, string role, CancellationToken cancellationToken = default)
        {
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);

                if (!roleResult.Succeeded)
                    return roleResult;
            }

            return result;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            =>  await userManager.UpdateAsync(user);

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
            => await userManager.CheckPasswordAsync(user, password);

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => await userManager.GeneratePasswordResetTokenAsync(user);

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken = default)
            => await userManager.ResetPasswordAsync(user, token, newPassword);

        public async Task<bool> IsVendorVerifiedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
           
                var vendor = await dbContext.Vendors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

                return vendor?.IsVerified ?? false;

        }
    }
}