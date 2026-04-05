using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Contracts
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetByStatusAsync(string status, CancellationToken cancellationToken);
        Task<Event> CreateAsync(Event entity, CancellationToken cancellationToken);
        Task<Event> UpdateAsync(Event entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);


        Task<EventItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken);
        Task<EventItem> UpdateItemAsync(EventItem item, CancellationToken cancellationToken);
    }
}
