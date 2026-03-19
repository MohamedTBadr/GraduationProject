using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrderRepository(ApplicationDbContext db) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await db.Orders.FindAsync([id], ct);

        public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
            => await db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Event)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default)
            => await db.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default)
            => await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntentId, ct);

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;
            order.CreatedAt = DateTime.UtcNow;
            await db.Orders.AddAsync(order, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct = default)
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var order = await db.Orders.FindAsync([id], ct);
            if (order is not null)
            {
                db.Orders.Remove(order);
                await db.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await db.Orders.AnyAsync(o => o.Id == id, ct);
    }
}