using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public ApplicationUser User { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type{ get; set; } 
        public string Title { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }


    public enum NotificationType
    {
      ACCOUNT_ACCEPTED,ACCOUNT_SUSPENDED, ORDER_PLACED, ORDER_REJECTED,PAYMENT_REJECTED,PAYMENT_ACCEPTED,ORDER_CANCELLED,ORDER_COMPLETED, EVENT_STATUS_UPDATED,EVENT_STATUS_DELETED,EVENT_ITEM_APPROVED,EVENT_ITEM_REJECTED, EVENT_COMPLETED
    }
}
