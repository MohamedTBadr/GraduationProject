using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<Event> GetByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.User)
                .Include(e => e.ServiceType)
                .Include(e => e.Location)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Event> GetByIdWithItemsAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.User)
                .Include(e => e.ServiceType)
                .Include(e => e.Location)
                .Include(e => e.EventItems)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context.Events
                .Include(e => e.User)
                .Include(e => e.ServiceType)
                .Include(e => e.Location)
                .Include(e => e.EventItems)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Events
                .Include(e => e.ServiceType)
                .Include(e => e.Location)
                .Include(e => e.EventItems)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByStatusAsync(string status)
        {
            return await _context.Events
                .Include(e => e.User)
                .Include(e => e.ServiceType)
                .Include(e => e.Location)
                .Where(e => e.EventStatus == status)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<Event> CreateAsync(Event entity)
        {
            entity.Id = Guid.NewGuid();
            await _context.Events.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Event> UpdateAsync(Event entity)
        {
            _context.Events.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Events.FindAsync(id);
            if (entity == null) return false;

            _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
