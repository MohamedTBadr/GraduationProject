using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AuthenticationDTOs
{
    public record LoginRequest([Required][EmailAddress]string email,[Required]string password, string? referralCode = null);

}
