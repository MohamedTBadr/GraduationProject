using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AuthenticationDTOs
{
    public record ResetPasswordRequest(string email, string token, string newPassword);
}
