using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    /// <summary>
    /// Represents a vendor entity.
    /// Not Completed, we need to add more properties to this class, such as Linking with User entity,
    /// and also we need to add more details about the vendor such as contact information, location, etc.
    /// </summary>
    public class Vendor
    {
        public int Id { get; set; }
        public string BusinessName { get; set; }
        public string OwnerName { get; set; }
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();

        public decimal YearsInBusiness { get; set; }

        public string Description { get; set; }
        public string PortfolioLink { get; set; }


    }
}
