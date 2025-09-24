using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AuthenticationDTOs
{
    public record SignUpRequest(string name,string email,string password);
    
}
