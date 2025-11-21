using BLL.DTOs.AuthenticationDTOs;
using BLL.Services.Interfaces;
using Common.Exceptions;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Services
{
    // Use an interface IRefreshTokenGenerator or System.Security.Cryptography instead of Guid 
    // for a high-security refresh token implementation, but Guid is simple for this example.
    public class AuthenticationService(UserManager<ApplicationUser> userManager,
                                     IConfiguration configuration,
                                     IOptions<JWTOptions> options,
                                     IEmailSender emailSender) : IAuthenticationService
    {
        // Define Refresh Token duration (e.g., 30 days)
        private const int RefreshTokenDurationDays = 30;

        public async Task<UserResponse> LogIn(LoginRequest loginRequest)
        {
            // 1. Check if User exists
            var user = await userManager.FindByEmailAsync(loginRequest.email) ??
                throw new UserNotFoundException(loginRequest.email);

            // 2. Validate password
            var isValid = await userManager.CheckPasswordAsync(user, loginRequest.password);

            if (isValid)
            {
                // 3. Generate Access Token and Refresh Token
                var accessToken = await GenerateAccessTokenAsync(user);
                var refreshToken = GenerateNewRefreshToken();

                // 4. Update user entity with the new Refresh Token
                await SetRefreshTokenAsync(user, refreshToken, RefreshTokenDurationDays);

                // 5. Return both tokens
                return new(user.UserName!, user.Email!, accessToken, refreshToken);
            }

            throw new UnauthorizedException("Invalid credentials.");
        }

        public async Task<bool> CheckIfEmailExists(string email) => (await userManager.FindByEmailAsync(email)) != null;

        public async Task<UserResponse> RegisterAsync(SignUpRequest request)
        {
            // Input validation and existing user checks
            if (await userManager.FindByNameAsync(request.name) != null)
            {
                throw new UserAlreadyExistException($"User name '{request.name}' already exists.");
            }
            if (await userManager.FindByEmailAsync(request.email) != null)
            {
                throw new UserAlreadyExistException($"Email '{request.email}' already registered.");
            }

            var user = new ApplicationUser
            {
                Email = request.email,
                UserName = request.name
            };

            // 1. Create User
            var result = await userManager.CreateAsync(user, request.password);

            if (result.Succeeded)
            {
                // 2. Generate and set Tokens
                var accessToken = await GenerateAccessTokenAsync(user);
                var refreshToken = GenerateNewRefreshToken();
                await SetRefreshTokenAsync(user, refreshToken, RefreshTokenDurationDays);

                // 3. Return both tokens
                return new(request.name, request.email, accessToken, refreshToken);
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException(errors);
        }

        public async Task ForgetPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email) ?? throw new UserNotFoundException(email);

            // 1) create token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            // 2) encode token so it's safe in a URL
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

            // 3) Build callback URL
            var baseUrl = configuration.GetSection("clientBaseUrl").Value;
            var callbackUrl = $"{baseUrl}Authentication/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

            // 4) Compose email (HTML) - Removed HTML for brevity but kept the structure
            var subject = "Reset your password";
            var body = $@"... Your full HTML email body here, using the {callbackUrl} ...";

            // 5) send email (use your IEmailSender implementation)
            await emailSender.SendEmailAsync(user.Email!, subject, body);
        }

        // 💡 NEW SECURE REFRESH LOGIC
        public async Task<UserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Find the user by the Refresh Token in the database
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                // Token not found or already revoked/used.
                throw new UnauthorizedException("Invalid refresh token.");
            }

            // 2. Check if the Refresh Token has expired
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                // The token is expired. Revoke it and force user to log in again.
                await ClearRefreshTokenAsync(user);
                throw new UnauthorizedException("Refresh token expired. Please log in again.");
            }

            // 3. Generate a new Access Token (short-lived)
            var newAccessToken = await GenerateAccessTokenAsync(user);

            // 4. Generate a new Refresh Token (Token Rotation)
            var newRefreshToken = GenerateNewRefreshToken();

            // 5. Update user with the new Refresh Token and expiry time (Revoke old token)
            await SetRefreshTokenAsync(user, newRefreshToken, RefreshTokenDurationDays);

            // 6. Return the new tokens
            return new UserResponse(user.UserName!, user.Email!, newAccessToken, newRefreshToken);
        }

        // --- Private Utility Methods ---

        /// <summary>
        /// Generates the short-lived JWT (Access Token).
        /// </summary>
        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var jwt = options.Value;
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
            };

            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                // Access Token duration from configuration
                expires: DateTime.UtcNow.AddDays(jwt.DurationDays),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a unique, cryptographically random string for the Refresh Token.
        /// </summary>
        private string GenerateNewRefreshToken()
        {
            // Using Base64Url-encoded cryptographically secure random bytes is standard.
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return WebEncoders.Base64UrlEncode(randomNumber);
        }

        /// <summary>
        /// Updates the user entity in the database with the new Refresh Token details.
        /// </summary>
        private async Task SetRefreshTokenAsync(ApplicationUser user, string token, int durationDays)
        {
            user.RefreshToken = token;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(durationDays);
            await userManager.UpdateAsync(user);
        }

        /// <summary>
        /// Clears the Refresh Token from the user entity (revocation).
        /// </summary>
        private async Task ClearRefreshTokenAsync(ApplicationUser user)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            await userManager.UpdateAsync(user);
        }
    }
}