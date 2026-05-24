using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AuthenticationDTOs
{
    public class JWTOptions
    {
        public string SecretKey {  get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double AccessTokenDurationDays { get; set; }
        public double RefreshTokenDurationDays { get; set; }
    }
}
