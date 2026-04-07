using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ServiceRating
    {
        public Guid Id { get; set; }
        public Service Service { get; set; }
        public Guid  ServiceId { get; set; }

        public ApplicationUser User { get; set; }   
        public Guid UserId { get; set; }
        public decimal Rating { get; set; }
        public string Review { get; set; }
    }
}
