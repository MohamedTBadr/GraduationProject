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
        private readonly ResiliencePipeline _pipeline;

        public EventItemRepository(ApplicationDbContext context, ResiliencePipelineProvider<string> pipelineProvider)
        {
            _context = context;
            _pipeline = pipelineProvider.GetPipeline("db-pipeline");
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<EventItem> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token => await _context.EventItems
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.Id == id, token), cancellationToken);
        }

        public async Task<IEnumerable<EventItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token => await _context.EventItems
                .Include(i => i.Event)
                .ToListAsync(token), cancellationToken);
        }

        public async Task<IEnumerable<EventItem>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token => await _context.EventItems
                .Where(i => i.EventId == eventId)
                .ToListAsync(token), cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token => await _context.EventItems.AnyAsync(i => i.Id == id, token), cancellationToken);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventItem> CreateAsync(EventItem entity, CancellationToken cancellationToken)
        {
            entity.Id = Guid.NewGuid();
            await _pipeline.ExecuteAsync(async token =>
            {
                await _context.EventItems.AddAsync(entity, token);
                await _context.SaveChangesAsync(token);
            }, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<EventItem>> CreateRangeAsync(IEnumerable<EventItem> entities, CancellationToken cancellationToken)
        {
            var list = new List<EventItem>();
            foreach (var item in entities)
            {
                item.Id = Guid.NewGuid();
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
            var entity = await _pipeline.ExecuteAsync(async token => await _context.EventItems.FindAsync([id], token), cancellationToken);
            if (entity == null) return false;

            _context.EventItems.Remove(entity);
            await _pipeline.ExecuteAsync(async token => await _context.SaveChangesAsync(token), cancellationToken);
            return true;
        }

        public async Task<bool> DeleteByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var items = await _pipeline.ExecuteAsync(async token => await _context.EventItems
                .Where(i => i.EventId == eventId)
                .ToListAsync(token), cancellationToken);
            if (!items.Any()) return false;

            _context.EventItems.RemoveRange(items);
            await _pipeline.ExecuteAsync(async token => await _context.SaveChangesAsync(token), cancellationToken);
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
