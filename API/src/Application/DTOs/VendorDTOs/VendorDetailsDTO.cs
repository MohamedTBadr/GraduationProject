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
        public string Email { get; set; }
        public bool IsVerified { get; set; }

        public string PhoneNumber { get; set; }
        public decimal YearsInBusiness { get; set; } 
        public decimal StartingPrice { get; set; }
        public ICollection<VendorRatingDTO> VendorRating { get; set; } = new List<VendorRatingDTO>();
         public ICollection<ServiceDTOs.ServiceDTO> Services { get; set; } = new List<ServiceDTOs.ServiceDTO>();
        public ICollection<PackageDTOs.PackageDTO> Packages { get; set; } = new List<PackageDTOs.PackageDTO>();

        public ICollection<ServiceAreaDTO> ServiceAreas { get; set; } = new List<ServiceAreaDTO>();
    }
}
