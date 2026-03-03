using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AuthenticationDTOs
{
    public record SignUpRequest([Required]string name,[Required][EmailAddress]string email,[Required]string password);
    
}
