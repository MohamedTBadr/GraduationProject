using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
            return await _context.Events
                .Include(e => e.Category)
                //.Include(e => e.Location)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Event?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Category)
                //.Include(e => e.Location)       // ← was commented out; mapper needs it
                .Include(e => e.EventItems)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Category)
                //.Include(e => e.Location)
                .Include(e => e.EventItems)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Category)
                //.Include(e => e.Location)       // ← was commented out
                .Include(e => e.EventItems)
                .Where(e => e.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Category)
                //.Include(e => e.Location)
                .Include(e => e.EventItems)     // ← was missing; SummaryDto needs ItemCount
                .Where(e => e.EventStatus == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Events.AnyAsync(e => e.Id == id, cancellationToken);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<Event> CreateAsync(Event entity, CancellationToken cancellationToken)
        {
            entity.Id = Guid.NewGuid();
            await _context.Events.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            // Re-fetch to ensure all navigations are loaded for the mapper
            return await _context.Events
                .Include(e => e.Category)
                //.Include(e => e.Location)
                .Include(e => e.EventItems)
                .FirstAsync(e => e.Id == entity.Id);
        }

        public async Task<Event> UpdateAsync(Event entity, CancellationToken cancellationToken)
        {
            _context.Events.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Re-fetch so mapper always has Category/Location/Items loaded
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Location)
                .Include(e => e.EventItems)
                .FirstAsync(e => e.Id == entity.Id);
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
            return await _context.EventItems
                .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        }

        public async Task<EventItem> UpdateItemAsync(EventItem item, CancellationToken cancellationToken)
        {
            _context.EventItems.Update(item);
            await _context.SaveChangesAsync(cancellationToken);
            return item;
        }
    }
}