using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.PaymobDTOs
{
    public record AuthResponse
    {
        public string token { get; set; }
    }

    public record OrderResponse
    {
        public long id { get; set; }
    }

    public record PaymentKeyResponse
    {
        public string token { get; set; }
    }

}
