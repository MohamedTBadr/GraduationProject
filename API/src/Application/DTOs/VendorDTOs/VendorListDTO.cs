using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.VendorDTOs
{
    public class VendorListDTO
    {
        public Guid UserId { get; set; }
        public string BusinessName { get; set; }
        public decimal Rating { get; set; }
        public string Description { get; set; }
        public string VendorType { get; set; }
        public decimal YearsInBusiness { get; set; }
        public decimal StartingPrice { get; set; }
        public bool IsVerified { get; set; }
    }
}
