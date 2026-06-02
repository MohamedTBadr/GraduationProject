using System;
using System.Collections.Generic;

namespace Application.DTOs.PackageDTOs
{
    public class UpdatePackageDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }

        public ICollection<Guid> ServiceIds { get; set; }
        public Guid VendorId { get; set; }
    }
}
