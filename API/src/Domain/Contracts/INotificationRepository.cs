using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);

    }
}
