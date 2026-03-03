using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    /// <summary>
    /// Represents a package entity.
    /// </summary>
    public class Package
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }

        public ICollection<string> Items { get; set; }
        public Vendor Vendor { get; set; }
        public Guid VendorId { get; set; }

    }
}
