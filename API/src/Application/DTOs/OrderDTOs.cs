using Domain.Entities;

namespace Application.DTOs.Orders
{
    // ─── Requests ────────────────────────────────────────────────────────────────

    public record CreateOrderRequest(
        Guid UserId,
        Guid? EventId,
        List<OrderItemRequest> Items,
        string Currency,
        AddressDto ShippingAddress,
        DateTime? Appointment
    );

    public record OrderItemRequest(
        Guid ServiceId,
        string ServiceName,
        int Quantity,
        decimal UnitPrice
    );

    public record AddressDto(
        string Street,
        string City,
        string State,
        string PostalCode
    );

    public record UpdateOrderStatusRequest(
        string PaymentStatus
    );

    // ─── Responses ───────────────────────────────────────────────────────────────

    public record OrderResponse(
        Guid Id,
        Guid? UserId,
        List<OrderItemResponse> Items,
        decimal Amount,
        string Currency,
        string? PaymentIntentId,
        string PaymentStatus,
        DateTime CreatedAt,
        DateTime? Appointment,
        Guid? EventId
    );

    public record OrderItemResponse(
        Guid Id
      ,
        string ServiceName,
        int Quantity,
        decimal Price,
        decimal SubTotal
    );
}