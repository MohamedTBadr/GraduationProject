using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByEmailAsync(string email,CancellationToken cancellationToken);
        Task<ApplicationUser?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task<ApplicationUser?> GetByIdAsync(string userId, CancellationToken cancellationToken);
        Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user, CancellationToken cancellationToken);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password, CancellationToken cancellationToken);
        Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken);

        // Vendor specific check logic moved here
        Task<bool> IsVendorVerifiedAsync(Guid userId, CancellationToken cancellationToken);
    }
}
