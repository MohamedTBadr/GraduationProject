using Application.DTOs.AuthenticationDTOs;
using Application.Interfaces;

using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    // Use an interface IRefreshTokenGenerator or System.Security.Cryptography instead of Guid 
    // for a high-security refresh token implementation, but Guid is simple for this example.
    public class AuthenticationService(UserManager<ApplicationUser> userManager,
                                     IConfiguration configuration,
                                     IOptions<JWTOptions> options,
                                     IEmailSender emailSender,
                                     ApplicationDbContext dbContext,
                                     SseConnectionManager sseManager) : IAuthenticationService
    {
        // Define Refresh Token duration (e.g., 30 days)
        private const int RefreshTokenDurationDays = 30;

        public async Task<UserResponse> LogIn(LoginRequest loginRequest)
        {
            // 1. Check if User exists
            var user = await userManager.FindByEmailAsync(loginRequest.email) ??
                throw new UserNotFoundException(loginRequest.email);

            if (user.IsSuspended)
                throw new UnauthorizedException("Your account is suspended. Please contact support.");

            // 2. Validate password
            var isValid = await userManager.CheckPasswordAsync(user, loginRequest.password);

            if (!isValid)
                throw new UnauthorizedException("Invalid credentials.");

            // 3. Check if vendor is verified
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            if (role == "Vendor")
            {
                var vendor = await dbContext.Vendors.FirstOrDefaultAsync(v => v.UserId == user.Id);

                if (vendor is null || !vendor.IsVerified)
                    throw new BadRequestException(new List<string> { "Your vendor account is not verified yet. Please wait for approval." });
            }

            // 4. Generate Access Token and Refresh Token
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = GenerateNewRefreshToken();

            // 5. Update user entity with the new Refresh Token
            await SetRefreshTokenAsync(user, refreshToken, RefreshTokenDurationDays);

            // 6. Return both tokens
            return new(user.UserName!, user.Email!, accessToken, refreshToken, role);
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
                FirstName = request.firstName,
                LastName = request.lastName,
                Email = request.email,
                UserName = request.name,
                PhoneNumber = request.phoneNumber
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
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? string.Empty;
                return new(request.name, request.email, accessToken, refreshToken, role);
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
            var callbackUrl = $"{baseUrl}reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

            // 4) Compose email (HTML) - Removed HTML for brevity but kept the structure
            var subject = "Reset your password";
            var body = $@"<!doctype html>
<html lang=""en"">
    <head>
        <meta charset=""UTF-8"" />
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
        <title>Email Verification</title>
    </head>
    <body style=""background-color: #f0ede6; margin: 0; padding: 0; box-sizing: border-box"">
        <div
            style=""
                padding: 40px 16px;
                display: flex;
                justify-content: center;
                align-items: flex-start;
            "">
            <div style=""width: 100%; max-width: 720px; margin: 0 auto"">
                <div
                    style=""
                        background-color: #2d5a4b;
                        border-radius: 16px 16px 0 0;
                        padding: 44px 40px 40px;
                        text-align: center;
                    "">
                    <h1 style=""font-size: 32px; color: #ffffff; line-height: 1.3; margin: 0"">
                        Welcome to Eventora, {user.UserName} !
                    </h1>
                </div>
                <div style=""background-color: #ffffff; padding: 40px 40px 36px"">
                    <p
                        style=""
                            font-size: 15px;
                            color: #3a3a3a;
                            line-height: 1.7;
                            margin: 0 0 20px 0;
                        "">
                        Dear <span style=""color: #c07c3a; font-weight: 600"">{user.UserName}</span>,
                    </p>
                    <p
                        style=""
                            font-size: 15px;
                            color: #3a3a3a;
                            line-height: 1.7;
                            margin: 0 0 20px 0;
                        "">
                        Thank you for registering with EpicHub! Please use the OTP below to verify
                        your email address and complete your registration:
                    </p>
                    <div style=""margin: 28px 0; text-align: center"">
                        <span
                            style=""
                                display: inline-block;

                                font-size: 42px;
                                color: #c07c3a;
                                letter-spacing: 0.15em;
                                background: #fdf6ed;
                                border: 1.5px solid #e8d5b8;
                                border-radius: 10px;
                                padding: 14px 36px;
                            "">
                          <a href=""{callbackUrl}"" style=""color: #c07c3a; text-decoration: none; font-weight: 600"">
                            Reset Password
                        </span>
                    </div>
                    <p
                        style=""
                            font-size: 15px;
                            color: #3a3a3a;
                            line-height: 1.7;
                            margin: 0 0 20px 0;
                        "">
                        If you did not request this registration, please ignore this email.
                    </p>
                    <hr style=""border: none; border-top: 1px solid #ebebeb; margin: 32px 0 28px"" />
                    <p style=""font-size: 14px; color: #5a5a5a; line-height: 1.6; margin: 0"">
                        Best regards,<br />
                        <strong>EpicHub Team</strong>
                    </p>
                </div>
                <div
                    style=""
                        background-color: #2d5a4b;
                        border-radius: 0 0 16px 16px;
                        padding: 28px 40px;
                        text-align: center;
                    "">
                    <p style=""font-size: 13px; color: #a8c9bc; margin: 0 0 6px 0"">
                        For support and updates, please visit our website or contact us via email.
                    </p>

                    <p style=""font-size: 13px; color: #a8c9bc; margin: 0"">
                        Email:
                        <a
                            href=""mailto:EpicHub@gmail.com""
                            style=""color: #7dcfb6; text-decoration: none; font-weight: 500"">
                            EpicHubhelp@gmail.com
                        </a>
                    </p>
                </div>
            </div>
        </div>
    </body>
</html>
";

            // 5) send email (use your IEmailSender implementation)
            await emailSender.SendEmailAsync(user.Email!, subject, body);
        }
        public async Task ResetPassword(ResetPasswordRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.email) ?? throw new UserNotFoundException(request.email);
            // 1) decode the token from URL
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            // 2) reset password
            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.newPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }
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
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;
            return new UserResponse(user.UserName!, user.Email!, newAccessToken, newRefreshToken, role);
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
                new(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ Add this - user Guid ID
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
                expires: DateTime.UtcNow.AddMinutes(jwt.DurationDays * 8),
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

        public async Task LogoutAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId.ToString());

            await ClearRefreshTokenAsync(user); // revoke refresh token
            sseManager.Remove(userId);          // close SSE connection
        }
    }
}