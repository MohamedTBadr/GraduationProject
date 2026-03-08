using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Domain.Entities;

namespace Domain.Contracts
{
    public interface IEventItemRepository
    {
        Task<EventItem> GetByIdAsync(Guid id);
        Task<IEnumerable<EventItem>> GetAllAsync();
        Task<IEnumerable<EventItem>> GetByEventIdAsync(Guid eventId);
        Task<EventItem> CreateAsync(EventItem entity);
        Task<IEnumerable<EventItem>> CreateRangeAsync(IEnumerable<EventItem> entities);
        Task<EventItem> UpdateAsync(EventItem entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteByEventIdAsync(Guid eventId);
        Task<bool> ExistsAsync(Guid id);
    }
}
