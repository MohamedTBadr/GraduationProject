using Application.DTOs.AuthenticationDTOs;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Exceptions;
using Shared.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    // Use an interface IRefreshTokenGenerator or System.Security.Cryptography instead of Guid 
    // for a high-security refresh token implementation, but Guid is simple for this example.
    public class AuthenticationService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IOptions<JWTOptions> options,
            IEmailSender emailSender,
                IVoucherService voucherService,   // ← add

            SseConnectionManager sseManager) : IAuthenticationService
    {
        // Define Refresh Token duration (e.g., 30 days)
        private const int RefreshTokenDurationDays = 30;
        public async Task<Result<UserResponse>> LogIn(LoginRequest loginRequest,CancellationToken cancellationToken)
        {
            // 1. Check if User exists via Repository
            var user = await userRepository.GetByEmailAsync(loginRequest.email, cancellationToken);
            if (user == null) {
               Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.NotFound(404, "User not found."));
                return failureResult;
            }

            if (user.IsSuspended)
            {
                 Result<UserResponse> result = Result<UserResponse>.Failure(Error.Unauthorized(401, "Your account got suspend "));
                return result;
            }
            // 2. Validate password via Repository
            var isValid = await userRepository.CheckPasswordAsync(user, loginRequest.password, cancellationToken);

            if (!isValid)
            {
                Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.Unauthorized(401, "Invalid credentials."));
                return failureResult;
            }

            // 3. Check roles and vendor verification
            var roles = await userRepository.GetUserRolesAsync(user, cancellationToken);
            var role = roles.FirstOrDefault() ?? string.Empty;

            if (role == "Vendor")
            {
                var isVerified = await userRepository.IsVendorVerifiedAsync(user.Id, cancellationToken);
                if (!isVerified)
                {
                    Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.BusinessRule(400, "Your vendor account is not verified yet. Please wait for approval."));
                    return failureResult;
                }
            }

            // 4. Generate Tokens
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = GenerateNewRefreshToken();

            // 5. Update user entity via Repository
            await SetRefreshTokenAsync(user, refreshToken, RefreshTokenDurationDays, cancellationToken);

            return Result<UserResponse>.Success(new UserResponse(user.UserName!, user.Email!, accessToken.Value, refreshToken, role));
        }

        public async Task<bool> CheckIfEmailExists(string email, CancellationToken cancellationToken)
                    => (await userRepository.GetByEmailAsync(email, cancellationToken)) != null;
        public async Task<Result<UserResponse>> RegisterAsync(SignUpRequest request, CancellationToken cancellationToken)
        {
            // Input validation and existing user checks
            if (await userRepository.GetByNameAsync(request.name, cancellationToken) != null)
            {
                Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.Conflict(409, $"Username '{request.name}' is already taken."));
                return failureResult;
            }
            if (await userRepository.GetByEmailAsync(request.email, cancellationToken) != null)
            {
                Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.Conflict(409, $"Email '{request.email}' is already registered."));
                return failureResult;
            }

            var user = new ApplicationUser
            {
                FirstName = request.firstName,
                LastName = request.lastName,
                Email = request.email,
                UserName = request.name,
                PhoneNumber = request.phoneNumber,
                ReferralCode = ReferralCodeGenerator.Generate() // Optional: Generate a referral code based on the username

            };

            // 1. Create User
            var result = await userRepository.CreateAsync(user, request.password, "Customer", cancellationToken);

            if (result.Succeeded)
            {

                // ← Apply referral reward if code was provided
                if (!string.IsNullOrEmpty(request.referralCode))
                    await voucherService.ApplyReferralAsync(request.referralCode, user.Id, cancellationToken);

                // 2. Generate and set Tokens
                var accessToken = await GenerateAccessTokenAsync(user);
                var refreshToken = GenerateNewRefreshToken();
                await SetRefreshTokenAsync(user, refreshToken, RefreshTokenDurationDays, cancellationToken);

                // 3. Return both tokens
                var roles = await userRepository.GetUserRolesAsync(user, cancellationToken);
                var role = roles.FirstOrDefault() ?? string.Empty;
                return Result<UserResponse>.Success(new UserResponse(request.name, request.email, accessToken.Value, refreshToken, role));
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new BadRequestException(errors);
        }

        public async Task ForgetPassword(string email, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByEmailAsync(email, cancellationToken) ?? throw new UserNotFoundException(email);

            // 1) create token
            var token = await userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);

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
                        background-color: linear-gradient(135deg, #161022 0%, #7a114c 100%);
                        border-radius: 16px 16px 0 0;
                        padding: 44px 40px 40px;
                        text-align: center;
                    "">
                    <h1 style=""font-size: 32px; color: #ffffff; line-height: 1.3; margin: 0"">
                        Welcome to EpicHub, {user.UserName} !
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
                          </a>
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
        public async Task ResetPassword(ResetPasswordRequest request,CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByEmailAsync(request.email,cancellationToken) ?? throw new UserNotFoundException(request.email);
            // 1) decode the token from URL
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            // 2) reset password
            var result = await userRepository.ResetPasswordAsync(user, decodedToken, request.newPassword, cancellationToken);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }
        }

        // 💡 NEW SECURE REFRESH LOGIC
        public async Task<Result<UserResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            // 1. Find the user by the Refresh Token in the database
            var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (user == null)
            {
                // Token not found or already revoked/used.
                 Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.Unauthorized(401, "Invalid refresh token. Please log in again."));
                return failureResult;
            }

            // 2. Check if the Refresh Token has expired
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                // The token is expired. Revoke it and force user to log in again.
                await ClearRefreshTokenAsync(user, cancellationToken);
                Result<UserResponse> failureResult = Result<UserResponse>.Failure(Error.Unauthorized(401, "Refresh token expired. Please log in again."));
                return failureResult;
                    }

            // 3. Generate a new Access Token (short-lived)
            var newAccessToken = await GenerateAccessTokenAsync(user);

            // 4. Generate a new Refresh Token (Token Rotation)
            var newRefreshToken = GenerateNewRefreshToken();

            // 5. Update user with the new Refresh Token and expiry time (Revoke old token)
            await SetRefreshTokenAsync(user, newRefreshToken, RefreshTokenDurationDays, cancellationToken);

            // 6. Return the new tokens
            var roles = await userRepository.GetUserRolesAsync(user, cancellationToken);
            var role = roles.FirstOrDefault() ?? string.Empty;
            return Result<UserResponse>.Success(new UserResponse(user.UserName!, user.Email!, newAccessToken.Value, newRefreshToken, role));
          
        }

        // --- Private Utility Methods ---

        /// <summary>
        /// Generates the short-lived JWT (Access Token).
        /// </summary>
        private async Task<Result<string>> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var jwt = options.Value;
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ Add this - user Guid ID
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
            };

            var roles = await userRepository.GetUserRolesAsync(user, CancellationToken.None);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                // Access Token duration from configuration
                expires: DateTime.UtcNow.AddDays(jwt.AccessTokenDurationDays),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);
            return Result<string>.Success(accessToken);
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
        private async Task SetRefreshTokenAsync(ApplicationUser user, string token, int durationDays, CancellationToken cancellationToken)
        {
            var jwt = options.Value;
            user.RefreshToken = token;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwt.RefreshTokenDurationDays);
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        /// <summary>
        /// Clears the Refresh Token from the user entity (revocation).
        /// </summary>
        private async Task ClearRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await ClearRefreshTokenAsync(user, cancellationToken); // revoke refresh token
            sseManager.Remove(userId);          // close SSE connection
        }
    }
}
