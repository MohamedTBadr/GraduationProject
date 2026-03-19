using Domain.Entities;

namespace Domain.Contracts
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
        Task UpdateAsync(Order order, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}