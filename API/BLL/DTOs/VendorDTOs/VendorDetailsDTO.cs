using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs.VendorDTOs
{
    public class VendorDetailsDTO
    {
        public string BusinessName { get; set; }
        public decimal Rating { get; set; }
        public string Description { get; set; }

        public decimal YearsInBusiness { get; set; }
        public decimal StartingPrice { get; set; }
        public List<VendorRating> VendorRating { get; set; } = new List<VendorRating>();
         public List<Product> Products { get; set; } = new List<Product>();
        public List<Package> Packages { get; set; } = new List<Package>();

    }
}
