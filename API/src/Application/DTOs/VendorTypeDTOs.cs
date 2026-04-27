using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class VendorTypeDetailsDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }



    public class CreateOrUpdateVendorTypeRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
