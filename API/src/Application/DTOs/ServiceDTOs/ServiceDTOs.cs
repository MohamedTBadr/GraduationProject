using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ServiceDTOs
{
    public record ServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        //public Guid EventTypeId { get; set; }
        //public string CategoryName { get; set; }
        public Guid VendorId { get; set; }
        public string VendorName { get; set; }
        public Guid ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public List<string> ServiceImages { get; set; }
        public decimal SetupDuration { get; set; } // in hours
        public decimal LeadTimeRequired { get; set; } // in weeks
    }

    public record CreateServiceRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> ServiceImages { get; set; }
        public List<Guid> EventTypeIds { get; set; }
        //public Guid CategoryId { get; set; }
        //CategoryId
        public Guid? VendorId { get; set; }
        public Guid ServiceTypeId { get; set; }
        public decimal SetupDuration { get; set; } // in hours
        public decimal LeadTimeRequired { get; set; } // in weeks
    }

    public record UpdateServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public Guid CategoryId { get; set; }
        public Guid? VendorId { get; set; }
        public List<IFormFile>? Images { get; set; } // ✅ nullable — if null, keep old

        public Guid ServiceTypeId { get; set; }
        public List<Guid> EventTypeIds { get; set; }
        public decimal SetupDuration { get; set; } // in hours
        public decimal LeadTimeRequired { get; set; } // in weeks
    }


    public record ServiceRatingRequest
    {
        public Guid? UserId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Rating { get; set; }
        public string Review { get; set; }
    }

}
