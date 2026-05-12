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
        ApplicationDbContext dbContext,
        ResiliencePipelineProvider<string> pipelineProvider) : IUserRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        // Identity UserManager doesn't always accept tokens for 'Find' methods, 
        // but we pass 'cancellationToken' to Polly to allow the pipeline to cancel retries.

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.FindByEmailAsync(email), cancellationToken);

        public async Task<ApplicationUser?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.FindByNameAsync(name), cancellationToken);

        public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.FindByIdAsync(userId.ToString()), cancellationToken);

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async token =>
                await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, token),
                cancellationToken);


        public async Task<ApplicationUser?> GetByReferralCodeAsync(string referralCode, CancellationToken ct) =>
    await dbContext.Users
            .FirstOrDefaultAsync(u => u.ReferralCode == referralCode, ct);

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.GetRolesAsync(user), cancellationToken);

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password, string role, CancellationToken cancellationToken = default)
        {
            var result = await _pipeline.ExecuteAsync(async _ => await userManager.CreateAsync(user, password), cancellationToken);

            if (result.Succeeded)
            {
                var roleResult = await _pipeline.ExecuteAsync(async _ => await userManager.AddToRoleAsync(user, role), cancellationToken);

                if (!roleResult.Succeeded)
                    return roleResult;
            }

            return result;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.UpdateAsync(user), cancellationToken);

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.CheckPasswordAsync(user, password), cancellationToken);

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.GeneratePasswordResetTokenAsync(user), cancellationToken);

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken = default)
            => await _pipeline.ExecuteAsync(async _ => await userManager.ResetPasswordAsync(user, token, newPassword), cancellationToken);

        public async Task<bool> IsVendorVerifiedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var vendor = await dbContext.Vendors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.UserId == userId, token);

                return vendor?.IsVerified ?? false;
            }, cancellationToken);
        }
    }
}