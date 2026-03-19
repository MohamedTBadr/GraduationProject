using Application.DTOs.AuthenticationDTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserResponse> LogIn(LoginRequest loginRequest);

        Task<UserResponse> RegisterAsync(SignUpRequest request);

        Task<bool> CheckIfEmailExists(string email);

        // 🚨 ENHANCEMENT: Changed method name and signature for secure Refresh Token flow.
        // It now accepts a DTO containing the old refresh token and returns new tokens.
        Task<UserResponse> RefreshTokenAsync(RefreshTokenRequest request);

        Task ForgetPassword(string email);
        Task ResetPassword(ResetPasswordRequest request);

        Task LogoutAsync(Guid userId);
    }
}