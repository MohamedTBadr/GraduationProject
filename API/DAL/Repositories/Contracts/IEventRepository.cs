using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(Guid id);
        Task<Event> GetByIdWithItemsAsync(Guid id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<IEnumerable<Event>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Event>> GetByStatusAsync(string status);
        Task<Event> CreateAsync(Event entity);
        Task<Event> UpdateAsync(Event entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
