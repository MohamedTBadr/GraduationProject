using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order: BaseEntity
    {
    public Guid Id { get; set; }
    public ApplicationUser User { get; set; }
    public Guid UserId { get; set; }


    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";

    public string? PaymentIntentId { get; set; } // Paymob order_id
    public string PaymentStatus { get; set; } = "Pending";

    public DateTime? Appointment { get; set; }

     public Event Event { get; set; }
        public Guid EventId { get; set; }
        public Address ShippingAddress { get; set; } = new Address();
    }

}
