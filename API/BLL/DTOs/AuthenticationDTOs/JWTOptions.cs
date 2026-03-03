using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AuthenticationDTOs
{
    public class JWTOptions
    {
        public string SecretKey {  get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double DurationDays { get; set; }
        
    }
}
