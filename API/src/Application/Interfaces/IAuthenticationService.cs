using Application.DTOs.AuthenticationDTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserResponse> LogIn(LoginRequest loginRequest, CancellationToken cancellationToken);

        Task<UserResponse> RegisterAsync(SignUpRequest request, CancellationToken cancellationToken);
        Task<bool> CheckIfEmailExists(string email, CancellationToken cancellationToken);

        // 🚨 ENHANCEMENT: Changed method name and signature for secure Refresh Token flow.
        // It now accepts a DTO containing the old refresh token and returns new tokens.
        Task<UserResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
        Task ForgetPassword(string email, CancellationToken cancellationToken);
        Task ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken);

        Task LogoutAsync(Guid userId, CancellationToken cancellationToken);
    }
}