using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class EventTypeRespository(ApplicationDbContext context) : IEventTypeRepository
    {
        public async Task CreateAsync(EventType eventType, CancellationToken cancellationToken)
        {
           await context.AddAsync(eventType,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(EventType eventType, CancellationToken cancellationToken)
        {

            context.EventTypes.Remove(eventType);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<EventType>> GetAllAsync(CancellationToken cancellationToken)
        {
            var types = context.EventTypes.AsNoTracking().ToList();
            return types;
        }

        public async Task<EventType> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var type = await context.EventTypes.FirstOrDefaultAsync(t => t.Id == id);
            return type!;
        }

        public async Task UpdateAsync(EventType eventType, CancellationToken cancellationToken)
        {
            context.EventTypes.Update(eventType);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
