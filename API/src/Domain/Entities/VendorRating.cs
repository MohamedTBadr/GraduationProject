using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class VendorRating
    {
        public int Id { get; set; }
        public Vendor Vendor { get; set; }
        public Guid  VendorId { get; set; }

        public ApplicationUser User { get; set; }   
        public Guid UserId { get; set; }
        public decimal Rating { get; set; }
        public string Review { get; set; }
    }
}
