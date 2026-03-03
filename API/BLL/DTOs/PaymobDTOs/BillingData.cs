using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.PaymobDTOs
{
    public class BillingData
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string city { get; set; } = "NA";
        public string country { get; set; } = "EG";
        public string street { get; set; } = "NA";
        public string building { get; set; } = "NA";
        public string floor { get; set; } = "NA";
        public string apartment { get; set; } = "NA";
        public string state { get; set; } = "NA";
        public string postal_code { get; set; } = "NA";
    }
  
}
