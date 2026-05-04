using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ServiceTypesDTOs
{
    public record ServiceTypeDTO
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid VendorTypeId { get; set; }

    }


    public record CreateServiceTypeRequest
    {
        public string Name { get; set; }
        public Guid VendorTypeId { get; set; }
    }


    public record UpdateServiceTypeRequest
    {
        public string Name { get; set; }
        public Guid VendorTypeId { get; set; }

    }
}
