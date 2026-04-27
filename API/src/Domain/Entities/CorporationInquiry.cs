using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CorporationInquiry
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public EventType EventType { get; set; }
        public Guid EventTypeId { get; set; }

        public DateTime ExpectedDate { get; set; }

        public int EstimatedAttendees { get; set; }

        public decimal ApproximateBudget { get; set; }

        public string AdditionalRequirements { get; set; }

        public string Status { get; set; }= "Pending";
    }
}
