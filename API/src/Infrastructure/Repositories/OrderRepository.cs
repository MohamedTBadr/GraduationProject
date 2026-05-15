using Domain.Contracts;
using Domain.Entities;
using Google.GenAI.Types;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class OrderRepository(ApplicationDbContext db) : IOrderRepository
    {


        public async Task<Event?> GetEventWithItemsAsync(Guid eventId, CancellationToken ct)
    => await db.Events
        .Include(e => e.EventItems)
        .FirstOrDefaultAsync(e => e.Id == eventId, ct);
        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
            => await db.Orders.FindAsync([id], ct);

        public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct )
            => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct)
            => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct)
            => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);
        public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct)
            => await db.Orders
                .Include(o => o.Event).ThenInclude(e => e.EventItems)
                .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntentId, ct);
        public async Task AddAsync(Order order, CancellationToken ct)
        {
            order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
            order.CreatedAt = DateTime.UtcNow;
            await db.Orders.AddAsync(order, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct)
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var order = await db.Orders.FindAsync([id], ct);
            if (order is not null)
            {
                db.Orders.Remove(order);
                await db.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
            => await db.Orders.AnyAsync(o => o.Id == id, ct);

        public async Task<decimal> GetOrderAmountAsync(Guid id,CancellationToken ct)
        {
            return await db.EventItems
                .Where(ei => ei.EventId == id)
                .SumAsync(ei => ei.Quantity * ei.Price, ct);
        }
    }
}