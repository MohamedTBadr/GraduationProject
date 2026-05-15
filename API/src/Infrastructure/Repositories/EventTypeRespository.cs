using Domain.Contracts;
using Domain.Entities;
using Google.GenAI.Types;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class EventTypeRespository(
        ApplicationDbContext context) : IEventTypeRepository
    {


        // Infrastructure/Repositories/EventTypeRepository.cs
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken )
        {
            
                return await context.EventTypes.AnyAsync(et => et.Id == id,cancellationToken);
        }
        public async Task CreateAsync(EventType eventType, CancellationToken cancellationToken)
        {
            
                await context.EventTypes.AddAsync(eventType, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(EventType eventType, CancellationToken cancellationToken)
        {
            
                context.EventTypes.Remove(eventType);
                await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<EventType>> GetAllAsync(CancellationToken cancellationToken)
        {
              return  await context.EventTypes
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
        }

        public async Task<EventType> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            
                var type = await context.EventTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
                return type!;
        }

        public async Task UpdateAsync(EventType eventType, CancellationToken cancellationToken)
        {
                            context.EventTypes.Update(eventType);
                await context.SaveChangesAsync(cancellationToken);
        }


    }
}