using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.VendorDTOs
{
    public class VendorDetailsDTO
    {
        public string BusinessName { get; set; }
        public decimal Rating { get; set; }
        public string Description { get; set; }

        public decimal YearsInBusiness { get; set; }
        public decimal StartingPrice { get; set; }
        //public List<VendorRatingDTOs.VendorRatingDTO> VendorRating { get; set; } = new List<VendorRatingDTOs.VendorRatingDTO>();
         public List<ProductDTOs.ProductDTO> Products { get; set; } = new List<ProductDTOs.ProductDTO>();
        public List<PackageDTOs.PackageDTO> Packages { get; set; } = new List<PackageDTOs.PackageDTO>();

    }
}
