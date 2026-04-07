using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Service
    {
       public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }
        public decimal Price { get; set; }
        public Guid ServiceTypeId { get; set; }

        public Category Category { get; set; }

        public Guid CategoryId { get; set; } 

         public decimal SetupDuration { get; set; } // in hours
        public decimal LeadTimeRequired { get; set; } // in weeks
        public Vendor Vendor { get; set; }
        public Guid VendorId { get; set; }

        public List<ServiceImage> ServiceImages { get; set; }

        public bool IsHidden { get; set; } = false;


        public ICollection<ServiceRating> ServiceRatings { get; set; }



    }
}
