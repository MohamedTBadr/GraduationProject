using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class OrderRepository(ApplicationDbContext db, ResiliencePipelineProvider<string> pipelineProvider) : IOrderRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _pipeline.ExecuteAsync(async token => await db.Orders.FindAsync([id], token), ct);

        public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
            => await _pipeline.ExecuteAsync(async token => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .FirstOrDefaultAsync(o => o.Id == id, token), ct);

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default)
            => await _pipeline.ExecuteAsync(async token => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .AsNoTracking()
                .ToListAsync(token), ct);

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _pipeline.ExecuteAsync(async token => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync(token), ct);
        public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default)
            => await _pipeline.ExecuteAsync(async token => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntentId, token), ct);
        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
            order.CreatedAt = DateTime.UtcNow;
            await _pipeline.ExecuteAsync(async token =>
            {
                await db.Orders.AddAsync(order, token);
                await db.SaveChangesAsync(token);
            }, ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct = default)
        {
            db.Orders.Update(order);
            await _pipeline.ExecuteAsync(async token => await db.SaveChangesAsync(token), ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var order = await _pipeline.ExecuteAsync(async token => await db.Orders.FindAsync([id], token), ct);
            if (order is not null)
            {
                db.Orders.Remove(order);
                await db.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await _pipeline.ExecuteAsync(async token => await db.Orders.AnyAsync(o => o.Id == id, token), ct);

        public async Task<decimal> GetOrderAmountAsync(Guid id,CancellationToken ct = default)
        {
            return await _pipeline.ExecuteAsync(async token => await db.EventItems
                .Where(ei => ei.EventId == id)
                .SumAsync(ei => ei.Quantity * ei.Price, token), ct);
        }
    }
}