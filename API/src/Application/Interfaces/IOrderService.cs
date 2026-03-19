using Application.DTOs.Orders;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
        Task<OrderResponse> GetOrderByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(CancellationToken ct = default);
        Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<OrderResponse> UpdatePaymentStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct = default);
        Task<OrderResponse> SetPaymentIntentAsync(Guid id, string paymentIntentId, CancellationToken ct = default);
        Task CancelOrderAsync(Guid id, CancellationToken ct = default);
        Task DeleteOrderAsync(Guid id, CancellationToken ct = default);
    }
}