using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.VendorDTOs
{
    public record UpdateVendorRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string BusinessName { get; set; }
        public string OwnerName { get; set; }
        public List<ServiceTypesDTOs.ServiceTypeDTO> ServiceTypes { get; set; } = new List<ServiceTypesDTOs.ServiceTypeDTO>();

        public IFormFile ProfilePicture { get; set; }
        public decimal YearsInBusiness { get; set; }

        public string Description { get; set; }
        public string PortfolioLink { get; set; }
        public AddressDto Address { get; set; }

        public ICollection<ServiceAreaDTO> ServiceAreas { get; set; } = new List<ServiceAreaDTO>();
    }
}
