using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IEventTypeRepository
    {
        Task<EventType> GetByIdAsync(Guid id,CancellationToken cancellationToken);
        Task<List<EventType>> GetAllAsync(CancellationToken cancellationToken);

        Task CreateAsync(EventType eventType,CancellationToken cancellationToken);
        Task UpdateAsync(EventType eventType,CancellationToken cancellationToken);
        Task DeleteAsync(EventType eventType, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    }
}
