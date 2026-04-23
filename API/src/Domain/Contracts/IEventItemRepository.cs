using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Domain.Entities;

namespace Domain.Contracts
{
    public interface IEventItemRepository
    {
        Task<EventItem> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<EventItem>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<EventItem>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
        Task<EventItem> CreateAsync(EventItem entity, CancellationToken cancellationToken);
        Task<IEnumerable<EventItem>> CreateRangeAsync(IEnumerable<EventItem> entities, CancellationToken cancellationToken);
        Task<EventItem> UpdateAsync(EventItem entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> DeleteByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

        Task<IEnumerable<EventItem>> GetVendorBookingsAsync(
    Guid vendorId,
    CancellationToken cancellationToken);
    }
}
