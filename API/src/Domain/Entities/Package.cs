using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a package entity.
    /// </summary>
    public class Package: BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }

        public ICollection<Guid> ServiceIds { get; set; }
        [NotMapped]
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public Vendor Vendor { get; set; }
        public Guid VendorId { get; set; }

    }
}
