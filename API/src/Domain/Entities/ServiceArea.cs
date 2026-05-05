using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ServiceArea
    {
        [Key]
        public Guid Id { get; set; }
        public Guid VendorId { get; set; } 
        public Vendor Vendor { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        // adding long , lattidue for areas
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

    }
}
