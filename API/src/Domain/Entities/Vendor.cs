using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a vendor entity.
    /// Not Completed, we need to add more properties to this class, such as Linking with User entity,
    /// and also we need to add more details about the vendor such as contact information, location, etc.
    /// </summary>
    public class Vendor: BaseEntity
    {
        [Key]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
       
        public string BusinessName { get; set; }
        public string ProfilePicture { get; set; }
        //public string OwnerName { get; set; }
        public ICollection<Service> Services { get; set; }
        public ICollection<Package> Packages { get; set; } 
        public ICollection<ServiceRating> VendorRatings { get; set; } 
        public decimal YearsInBusiness { get; set; }

        public string Description { get; set; }
        public string PortfolioLink { get; set; }
        public Address Address { get; set; }
        public bool IsVerified { get; set; }

        public VendorType VendorType { get; set; }
        public Guid VendorTypeId { get; set; }

        public ICollection<ServiceArea> ServiceAreas { get; set; }


        public string Document { get; set; }


    }
}
