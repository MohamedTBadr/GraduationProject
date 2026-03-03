using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs.VendorDTOs
{
    public class VendorListDTO
    {
        public string BusinessName { get; set; }
        public decimal Rating { get; set; }
        public string Description { get; set; }
        public string ServiceType { get; set; }
        public decimal YearsInBusiness { get; set; }
        public decimal StartingPrice { get; set; }
    }
}
