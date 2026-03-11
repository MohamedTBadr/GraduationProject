using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Contracts;
using Infrastructure.Persistence;
namespace Infrastructure.Repositories
{
    public class EventItemRepository : IEventItemRepository
    {
        private readonly ApplicationDbContext _context;

        public EventItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<EventItem> GetByIdAsync(Guid id)
        {
            return await _context.EventItems
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<EventItem>> GetAllAsync()
        {
            return await _context.EventItems
                .Include(i => i.Event)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventItem>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.EventItems
                .Where(i => i.EventId == eventId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.EventItems.AnyAsync(i => i.Id == id);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventItem> CreateAsync(EventItem entity)
        {
            entity.Id = Guid.NewGuid();
            await _context.EventItems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<EventItem>> CreateRangeAsync(IEnumerable<EventItem> entities)
        {
            var list = new List<EventItem>();
            foreach (var item in entities)
            {
                item.Id = Guid.NewGuid();
                list.Add(item);
            }
            await _context.EventItems.AddRangeAsync(list);
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<EventItem> UpdateAsync(EventItem entity)
        {
            _context.EventItems.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.EventItems.FindAsync(id);
            if (entity == null) return false;

            _context.EventItems.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByEventIdAsync(Guid eventId)
        {
            var items = await _context.EventItems
                .Where(i => i.EventId == eventId)
                .ToListAsync();

            if (!items.Any()) return false;

            _context.EventItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
