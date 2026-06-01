using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Contracts;
using Infrastructure.Persistence;
using Polly;
using Polly.Registry;
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

        public async Task<EventItem> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.EventItems
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.Id == id,cancellationToken);
        }
        public async Task<IEnumerable<EventItem>> GetVendorBookingsAsync(
          Guid vendorId,
          CancellationToken cancellationToken)
        {
            return await _context.EventItems
                .Where(ei => (ei.Service != null ? ei.Service.VendorId : ei.Package != null ? ei.Package.VendorId : Guid.Empty) == vendorId && ei.Event.Order != null) // ✅ one combined filter
                .Include(ei => ei.Event)
                    .ThenInclude(e => e.EventType)
                .Include(ei => ei.Event)
                    .ThenInclude(e => e.Location)
                .OrderByDescending(ei => ei.Event.EventDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<EventItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return  await _context.EventItems
                .Include(i => i.Event)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EventItem>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.EventItems
                .Where(i => i.EventId == eventId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return  await _context.EventItems.AnyAsync(i => i.Id == id,cancellationToken);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventItem> CreateAsync(EventItem entity, CancellationToken cancellationToken)
        {
            entity.Id = Guid.NewGuid();
            entity.Price = entity.Service?.Price ?? entity.Package?.Price ?? 0;   // ← snapshot at booking time

            await _context.EventItems.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<EventItem>> CreateRangeAsync(IEnumerable<EventItem> entities, CancellationToken cancellationToken)
        {
            var list = new List<EventItem>();

            foreach (var item in entities)
            {
                item.Id = Guid.NewGuid();
                item.Price = item.Service?.Price ?? item.Package?.Price ?? 0;   // ← snapshot per item
                list.Add(item);
            }

            await _context.EventItems.AddRangeAsync(list, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return list;
        }

        public async Task<EventItem> UpdateAsync(EventItem entity, CancellationToken cancellationToken)
        {
            _context.EventItems.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _context.EventItems.FindAsync([id], cancellationToken);
            if (entity == null) return false;

            _context.EventItems.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var items = await _context.EventItems
                .Where(i => i.EventId == eventId)
                .ToListAsync(cancellationToken);
            if (!items.Any()) return false;

            _context.EventItems.RemoveRange(items);
           await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        // EventRepository — add AddItemAsync (the other two already exist)
        public async Task<EventItem> AddItemAsync(EventItem item, CancellationToken cancellationToken)
        {
            item.Id = Guid.NewGuid();
            await _context.EventItems.AddAsync(item, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return item;
        }
    }
}
