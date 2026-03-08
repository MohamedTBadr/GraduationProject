using DAL.Entities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.VendorDTOs
{
    public record CreateVendorRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string BusinessName { get; set; }
        public string OwnerName { get; set; }
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();

        public decimal YearsInBusiness { get; set; }

        public string Description { get; set; }
        public string PortfolioLink { get; set; }
        public Address Address { get; set; }
    }
}
