using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PaymobDTOs
{
    public class PaymobOptions
    {
        public string ApiKey { get; set; }
        public int IntegrationId { get; set; }
        public int IframeId { get; set; }
        public string BaseUrl { get; set; }
        public string HmacSecret { get; set; }
    }

}
