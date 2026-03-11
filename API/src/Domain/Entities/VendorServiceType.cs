using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class VendorServiceType
    {
        public Vendor Vendor { get; set; }
        public Guid VendorId { get; set; }
        public ServiceType ServiceType { get; set; }
        public Guid ServiceTypeId { get; set; } 

    }
}
