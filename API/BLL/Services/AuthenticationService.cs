using BLL.DTOs.AuthenticationDTOs;
using BLL.Services.Interfaces;
using Common.Exceptions;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Services
{
    public class AuthenticationService(UserManager<ApplicationUser> userManager,IConfiguration configuration,IOptions<JWTOptions> options,IEmailSender emailSender) : IAuthenticationService
    {
        public async Task<UserResponse> LogIn(LoginRequest loginRequest)
        {
            //check if User exist 
            var User = await userManager.FindByEmailAsync(loginRequest.email)??
                throw new UserNotFoundException(loginRequest.email);
            var isValid= await userManager.CheckPasswordAsync(User, loginRequest.password);
            if (isValid) return new(User.UserName,User.Email, await GenerateTokenAsync(User));

            throw new UnauthorizedException();
        }
        public async Task<bool> CheckIfEmailExists(string email) =>( await userManager.FindByEmailAsync(email)) != null;


        public async Task<UserResponse> RegisterAsync(SignUpRequest request)
        {

            if (await userManager.FindByNameAsync(request.name) != null)
            {
                throw new UserAlreadyExistException(request.name);
            }
            if (await userManager.FindByEmailAsync(request.email) != null)
            {
                throw new UserAlreadyExistException(request.email);
            }

            var User = new ApplicationUser
            {
                Email = request.email,
                UserName = request.name
            };
            var Result = await userManager.CreateAsync(User, request.password);
            if (Result.Succeeded) return new(request.email, request.password, await GenerateTokenAsync(User));
            var errors = Result.Errors.Select(e => e.Description).ToList();
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
            // Example redirect to front-end page: /account/reset-password (or to MVC action)
            var callbackUrl = $"{baseUrl}Authentication/reset-password?email={Uri.EscapeDataString(user.Email)}&token={encodedToken}";

            // 4) Compose email (HTML)
            var subject = "Reset your password";
            var body = $@"
<table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#f4f4f4;padding:20px 0;'>
  <tr>
    <td align='center'>
      <table width='600' cellpadding='0' cellspacing='0' border='0' style='background-color:#ffffff;border-radius:8px;padding:20px;font-family:Arial,sans-serif;color:#333333;'>
        <tr>
          <td align='center' style='padding:20px 0;'>
            <h2 style='margin:0;color:#4CAF50;'>Reset Your Password</h2>
          </td>
        </tr>
        <tr>
          <td style='padding:20px;font-size:15px;line-height:1.6;'>
            <p>Hi,</p>
            <p>We received a request to reset your password. Click the button below to continue. 
               This link will expire after a short time for security reasons.</p>
            <p style='text-align:center;margin:30px 0;'>
              <a href='{callbackUrl}' 
                 style='display:inline-block;padding:12px 24px;background-color:#4CAF50;
                        color:#ffffff;text-decoration:none;border-radius:6px;font-weight:bold;'>
                 Reset Password
              </a>
            </p>
            <p>If you didn’t request this, you can safely ignore this email.</p>
            <p style='margin-top:30px;font-size:13px;color:#777;'>Thank you,<br/>The Support Team</p>
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>";


            // 5) send email (use your IEmailSender implementation)
            await emailSender.SendEmailAsync(user.Email, subject, body);


        }
        private async Task<string> GenerateTokenAsync(ApplicationUser User)
        {
            var jwt = options.Value;
            var Claims = new List<Claim>()
            {
                new(ClaimTypes.Name ,User.UserName),
                new(ClaimTypes.Email , User.Email),

            };
            var Roles= await userManager.GetRolesAsync(User);
            foreach (var item in Roles)
            {
                Claims.Add(new(ClaimTypes.Role, item));
            }
            

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: Claims,
            expires: DateTime.UtcNow.AddDays(jwt.DurationDays),
            signingCredentials: cred
            );
            var TokenHandler = new JwtSecurityTokenHandler();
            return TokenHandler.WriteToken(token);



        }


        public async Task<UserResponse> GenerateRefreshToken(string email)
        {
            var User=await userManager.FindByEmailAsync(email);


            return new UserResponse(User.UserName,
                 User.Email,
               await GenerateTokenAsync(User));
          
        }
    }
}
