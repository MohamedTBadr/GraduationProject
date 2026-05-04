using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a Service type entity.
    /// </summary>
    public class ServiceType
    {

        public Guid Id { get; set; }
        public string Name { get; set; }

        public VendorType VendorType { get; set; }
        public Guid VendorTypeId { get; set; }

    }
}
