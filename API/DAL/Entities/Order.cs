using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
   public class Order
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }


    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";

    public string? PaymentIntentId { get; set; } // Paymob order_id
    public string PaymentStatus { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}


}
