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
        private readonly ResiliencePipeline _pipeline;

        public EventRepository(ApplicationDbContext context, ResiliencePipelineProvider<string> pipelineProvider)
        {
            _context = context;
            _pipeline = pipelineProvider.GetPipeline("db-pipeline");
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.Collaborators)
                    .FirstOrDefaultAsync(e => e.Id == id, token),
                cancellationToken);
        }

        public async Task<Event?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .Include(e => e.Collaborators)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(e => e.Id == id, token),
                cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .ToListAsync(token),
                cancellationToken);
        }

 

        public async Task<IEnumerable<Event>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .Where(e => e.UserId == userId || e.Collaborators.Any(c => c.UserId == userId))
                    .ToListAsync(token),
                cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .Where(e => e.EventStatus == status)
                    .ToListAsync(token),
                cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Events.AnyAsync(e => e.Id == id, token),
                cancellationToken);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<Event> CreateAsync(Event entity, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                entity.Id = Guid.NewGuid();
                await _context.Events.AddAsync(entity, token);
                await _context.SaveChangesAsync(token);

                return await _context.Events
                    .Include(e => e.EventType)
                    .Include(e => e.EventItems)
                    .FirstAsync(e => e.Id == entity.Id, token);
            }, cancellationToken);
        }

        public async Task<Event> UpdateAsync(Event entity, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                _context.Events.Update(entity);
                await _context.SaveChangesAsync(token);

                return await _context.Events
                    .Include(e => e.EventType)
                    //.Include(e => e.Location)?
                    .Include(e => e.EventItems)
                    .FirstAsync(e => e.Id == entity.Id, token);
            }, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var entity = await _context.Events.FindAsync([id], token);
                if (entity == null) return false;

                _context.Events.Remove(entity);
                await _context.SaveChangesAsync(token);
                return true;
            }, cancellationToken);
        }

        public async Task<EventItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.EventItems
                    .FirstOrDefaultAsync(i => i.Id == itemId, token),
                cancellationToken);
        }

        public async Task<EventItem> UpdateItemAsync(EventItem item, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                _context.EventItems.Update(item);
                await _context.SaveChangesAsync(token);
                return item;
            }, cancellationToken);
        }

        public async Task<EventItem> AddItemAsync(EventItem item, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                await _context.EventItems.AddAsync(item, token);
                await _context.SaveChangesAsync(token);
                return item;
            }, cancellationToken);
        }

        public async Task AddCollaboratorAsync(EventCollaborator collaborator, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                await _context.EventCollaborators.AddAsync(collaborator, token);
                await _context.SaveChangesAsync(token);
            }, cancellationToken);
        }

        public async Task RemoveCollaboratorAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                var collab = await _context.EventCollaborators
                    .FirstOrDefaultAsync(ec => ec.EventId == eventId && ec.UserId == userId, token);
                if (collab != null)
                {
                    _context.EventCollaborators.Remove(collab);
                    await _context.SaveChangesAsync(token);
                }
            }, cancellationToken);
        }

        public async Task<IEnumerable<EventCollaborator>> GetCollaboratorsAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.EventCollaborators
                    .Include(ec => ec.User)
                    .Where(ec => ec.EventId == eventId)
                    .ToListAsync(token),
                cancellationToken);
        }

        public async Task<EventCollaborator?> GetCollaboratorAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.EventCollaborators
                    .FirstOrDefaultAsync(ec => ec.EventId == eventId && ec.UserId == userId, token),
                cancellationToken);
        }
    }
}