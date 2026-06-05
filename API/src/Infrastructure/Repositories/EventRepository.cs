using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
              return  await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.Collaborators)
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Event?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.User)
                .Include(e => e.EventType)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Service)
                        .ThenInclude(s => s.ServiceImages)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Service)
                        .ThenInclude(s => s.Vendor)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Package)

                .Include(e => e.Collaborators)
                    .ThenInclude(c => c.User)

                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }


        public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.EventType)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Service)
                        .ThenInclude(s => s.ServiceImages)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Service)
                        .ThenInclude(s => s.Vendor)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Package)

                .ToListAsync(cancellationToken);
        }


        public async Task<IEnumerable<Event>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.User)
                .Include(e => e.EventType)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Service)
                        .ThenInclude(s => s.ServiceImages)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Service)
                        .ThenInclude(s => s.Vendor)

                .Include(e => e.EventItems)
                    .ThenInclude(x => x.Package)

                .Where(e => e.UserId == userId ||
                            e.Collaborators.Any(c => c.UserId == userId))

                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            return 
                await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .Where(e => e.EventStatus == status)
                    .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return 
                await _context.Events.AnyAsync(e => e.Id == id,cancellationToken);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<Event> CreateAsync(Event entity, CancellationToken cancellationToken)
        {
          
                entity.Id = Guid.NewGuid();
                await _context.Events.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .FirstAsync(e => e.Id == entity.Id, cancellationToken);
        }

        public async Task<Event> UpdateAsync(Event entity, CancellationToken cancellationToken)
        {
            
                _context.Events.Update(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return await _context.Events
                    .Include(e => e.EventType)
                    //.Include(e => e.Location)?
                    .Include(e => e.EventItems)
                    .FirstAsync(e => e.Id == entity.Id, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
          
                var entity = await _context.Events.FindAsync([id], cancellationToken);
                if (entity == null) return false;

                _context.Events.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
        }

        public async Task<EventItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken)
        {
            return 
                await _context.EventItems
                    .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        }

        public async Task<EventItem> UpdateItemAsync(EventItem item, CancellationToken cancellationToken)
        {
            
                _context.EventItems.Update(item);
                await _context.SaveChangesAsync(cancellationToken);
                return item;
        }

        public async Task<EventItem> AddItemAsync(EventItem item, CancellationToken cancellationToken)
        {
            
                await _context.EventItems.AddAsync(item, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return item;
        }

        public async Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken)
        {
            var item = await _context.EventItems.FindAsync([itemId], cancellationToken);
            if (item == null) return false;

            _context.EventItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task AddCollaboratorAsync(EventCollaborator collaborator, CancellationToken cancellationToken)
        {
            
                await _context.EventCollaborators.AddAsync(collaborator, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveCollaboratorAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
        {
            
                var collab = await _context.EventCollaborators
                    .FirstOrDefaultAsync(ec => ec.EventId == eventId && ec.UserId == userId, cancellationToken);
                if (collab != null)
                {
                    _context.EventCollaborators.Remove(collab);
                    await _context.SaveChangesAsync(cancellationToken);
                }
        }

        public async Task<IEnumerable<EventCollaborator>> GetCollaboratorsAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return
                await _context.EventCollaborators
                    .Include(ec => ec.User)
                    .Where(ec => ec.EventId == eventId)
                    .ToListAsync(cancellationToken);
        }

        public async Task<EventCollaborator?> GetCollaboratorAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
        {
            return 
                await _context.EventCollaborators
                    .FirstOrDefaultAsync(ec => ec.EventId == eventId && ec.UserId == userId, cancellationToken);
        }
    }
}
