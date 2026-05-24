using Application.DTOs.Orders;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Result<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct );
        Task<Result<OrderResponse>> GetOrderByIdAsync(Guid id, CancellationToken ct );
        Task<Result<IEnumerable<OrderResponse>>> GetAllOrdersAsync(CancellationToken ct );
        Task<Result<IEnumerable<OrderResponse>>> GetOrdersByUserIdAsync(Guid userId, CancellationToken ct );
        Task<Result<OrderResponse>> UpdatePaymentStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct);
        Task<Result<OrderResponse>> SetPaymentIntentAsync(Guid id, string paymentIntentId, CancellationToken ct);
        Task<Result<bool>> CancelOrderAsync(Guid id, CancellationToken ct);
        Task<Result<bool>> DeleteOrderAsync(Guid id, CancellationToken ct);
    }
}