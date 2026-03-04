using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Event
    {

        public Guid Id { get; set; }

        public ApplicationUser User { get; set; }
        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public ServiceType ServiceType { get; set; }
        public Guid ServiceTypeId { get; set; }
        public DateTime EventDate { get; set; }
        public Address Location { get; set; }

        public decimal TotalBudget { get; set; }
        public int GuestCount { get; set; }

        public string Notes {  get; set; }

        public string EventStatus { get; set; } = "Planned"; // Planned, Completed, Cancelled
    }
}
