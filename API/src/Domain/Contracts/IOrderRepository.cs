using Domain.Entities;

namespace Domain.Contracts
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct );
        Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct );
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct );
        Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct );
        Task<decimal> GetOrderAmountAsync(Guid id, CancellationToken ct );
        Task AddAsync(Order order, CancellationToken ct );
        Task UpdateAsync(Order order, CancellationToken ct );
        Task DeleteAsync(Guid id, CancellationToken ct );
        Task<bool> ExistsAsync(Guid id, CancellationToken ct);
        Task<Event?> GetEventWithItemsAsync(Guid eventId, CancellationToken ct);

    }
}