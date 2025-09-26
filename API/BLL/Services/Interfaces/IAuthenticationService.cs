using BLL.DTOs.AuthenticationDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<UserResponse> LogIn(LoginRequest loginRequest);

        Task<UserResponse> RegisterAsync(SignUpRequest request);
        Task<bool> CheckIfEmailExists(string email);
        Task<UserResponse> GenerateRefreshToken(string email); 
        Task ForgetPassword(string email);
    }
}
