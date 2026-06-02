using Application.DTOs.ServiceDTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PackageDTOs
{
    public class PackageDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }

        public ICollection<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();
        public Guid VendorId { get; set; }
    }
}
