using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AuthenticationDTOs
{
    public record UserResponse(string name,string email,string AccessToken,string RefreshToken,string role);
    
}
