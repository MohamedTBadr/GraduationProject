using Application.DTOs.Orders;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct );
        Task<OrderResponse> GetOrderByIdAsync(Guid id, CancellationToken ct );
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(CancellationToken ct );
        Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid userId, CancellationToken ct );
        Task<OrderResponse> UpdatePaymentStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct);
        Task<OrderResponse> SetPaymentIntentAsync(Guid id, string paymentIntentId, CancellationToken ct);
        Task CancelOrderAsync(Guid id, CancellationToken ct);
        Task DeleteOrderAsync(Guid id, CancellationToken ct);
    }
}