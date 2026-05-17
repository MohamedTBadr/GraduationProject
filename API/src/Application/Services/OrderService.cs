using Application.DTOs.Orders;

using Application.Interfaces.Services;
using Domain.Contracts;
using Domain.Entities;

namespace Application.Services
{
    public class OrderService(
        IOrderRepository orderRepo,
        NotificationService notificationService,IVoucherService voucherService) : IOrderService
    {
        // ─── Create ───────────────────────────────────────────────────────────────

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct )
        {
            if (request.ShippingAddress is null)
                throw new ArgumentException("Shipping address is required.", nameof(request));

            // 1. Fetch event and validate ownership (IDOR Fix)
            var eventWithItems = await orderRepo.GetEventWithItemsAsync(request.EventId, ct)
                ?? throw new KeyNotFoundException($"Event {request.EventId} was not found.");

            if (eventWithItems.UserId != request.UserId)
                throw new UnauthorizedAccessException("You are not authorized to create an order for this event.");

            // 2. Calculate base amount in-memory (Performance Optimization)
            var amount = eventWithItems.EventItems?.Sum(ei => ei.Quantity * ei.Price) ?? 0;

            // 3. Apply voucher discount if provided
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var voucherResult = await voucherService.ValidateVoucherAsync(
                    request.VoucherCode, request.UserId, ct);

                if (!voucherResult.IsValid)
                    throw new InvalidOperationException(voucherResult.ErrorMessage);

                var discount = amount * (voucherResult.DiscountPercent / 100);
                amount -= discount;

                await voucherService.MarkVoucherUsedAsync(request.VoucherCode, ct);
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                EventId = request.EventId,
                Amount = amount,                        // ← discounted amount
                Currency = request.Currency ?? "EGP",
                Appointment = request.Appointment,
                VoucherCode = request.VoucherCode,      // ← store applied voucher code
                ShippingAddress = new Address
                {
                    Street = request.ShippingAddress.Street,
                    City = request.ShippingAddress.City,
                    State = request.ShippingAddress.State,
                }
            };

            await orderRepo.AddAsync(order, ct);

            if (order.UserId != Guid.Empty)
            {
                await notificationService.SendAsync(
                    order.UserId,
                    nameof(NotificationType.ORDER_PLACED),
                    "Order Placed",
                    $"Your order #{order.Id} has been placed successfully. Total: {order.Amount} {order.Currency}.");

                // Reuse the previously loaded eventWithItems instead of querying SQL again!
                if (eventWithItems.EventItems is { Count: > 0 })
                {
                    var vendorNotifications = eventWithItems.EventItems
                        .GroupBy(i => i.VendorId)
                        .Select(g => (
                            UserId: g.Key,
                            Type: nameof(NotificationType.ORDER_PLACED),
                            Title: "Order Details",
                            Message: $"Order #{order.Id} details: {string.Join(", ", g.Select(i => $"{i.Quantity}x {i.ServiceName}"))}."))
                        .ToList();

                    await notificationService.SendBulkAsync(vendorNotifications);
                }
            }

            return MapToResponse(order);
        }

        // ─── Read ─────────────────────────────────────────────────────────────────

        public async Task<OrderResponse> GetOrderByIdAsync(Guid id, CancellationToken ct )
        {
            var order = await orderRepo.GetByIdWithItemsAsync(id, ct)
                ?? throw new KeyNotFoundException($"Order {id} not found.");
            return MapToResponse(order);
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(CancellationToken ct )
        {
            var orders = await orderRepo.GetAllAsync(ct);
            return orders.Select(MapToResponse);
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(Guid userId, CancellationToken ct )
        {
            var orders = await orderRepo.GetByUserIdAsync(userId, ct);
            return orders.Select(MapToResponse);
        }

        // ─── Update ───────────────────────────────────────────────────────────────

        public async Task<OrderResponse> UpdatePaymentStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct )
        {
            var order = await orderRepo.GetByIdWithItemsAsync(id, ct)
                ?? throw new KeyNotFoundException($"Order {id} not found.");

            // Idempotency check: if status is already the same, skip processing
            if (order.PaymentStatus.Equals(request.PaymentStatus, StringComparison.OrdinalIgnoreCase))
                return MapToResponse(order);

            order.PaymentStatus = request.PaymentStatus;
            await orderRepo.UpdateAsync(order, ct);

            // Revert voucher if payment fails or is rejected
            if (request.PaymentStatus is "Failed" or "Rejected" && !string.IsNullOrEmpty(order.VoucherCode))
            {
                await voucherService.MarkVoucherUnusedAsync(order.VoucherCode, ct);
            }

            if (order.UserId != Guid.Empty)
            {
                var (type, title, msg) = request.PaymentStatus switch
                {
                    "Paid" => (NotificationType.PAYMENT_ACCEPTED, "Payment Accepted", $"Payment for order #{order.Id} was accepted."),
                    "Failed" => (NotificationType.PAYMENT_REJECTED, "Payment Rejected", $"Payment for order #{order.Id} was rejected. Please retry."),
                    "Completed" => (NotificationType.ORDER_COMPLETED, "Order Completed", $"Order #{order.Id} has been completed."),
                    "Rejected" => (NotificationType.ORDER_REJECTED, "Order Rejected", $"Order #{order.Id} was rejected."),
                    _ => ((NotificationType?)null, (string?)null, (string?)null)
                };

                if (type.HasValue)
                    await notificationService.SendAsync(order.UserId, type.Value.ToString(), title!, msg!);
            }

            return MapToResponse(order);
        }

        public async Task<OrderResponse> SetPaymentIntentAsync(Guid id, string paymentIntentId, CancellationToken ct )
        {
            var order = await orderRepo.GetByIdWithItemsAsync(id, ct)
                ?? throw new KeyNotFoundException($"Order {id} not found.");

            order.PaymentIntentId = paymentIntentId;
            await orderRepo.UpdateAsync(order, ct);
            return MapToResponse(order);
        }

        // ─── Cancel / Delete ──────────────────────────────────────────────────────

        public async Task CancelOrderAsync(Guid id, CancellationToken ct)
        {
            var order = await orderRepo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Order {id} not found.");

            if (order.PaymentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return;

            order.PaymentStatus = "Cancelled";
            await orderRepo.UpdateAsync(order, ct);

            // Revert voucher if order is cancelled
            if (!string.IsNullOrEmpty(order.VoucherCode))
            {
                await voucherService.MarkVoucherUnusedAsync(order.VoucherCode, ct);
            }

            if (order.UserId != Guid.Empty)
                await notificationService.SendAsync(
                    order.UserId,
                    nameof(NotificationType.ORDER_CANCELLED),
                    "Order Cancelled",
                    $"Your order #{order.Id} has been cancelled.");
        }

        public async Task DeleteOrderAsync(Guid id, CancellationToken ct )
        {
            if (!await orderRepo.ExistsAsync(id, ct))
                throw new KeyNotFoundException($"Order {id} not found.");

            await orderRepo.DeleteAsync(id, ct);
        }

        // ─── Mapping ──────────────────────────────────────────────────────────────

        private static OrderResponse MapToResponse(Order o) => new(
            o.Id,
            o.UserId,
            o.Amount,
            o.Currency,
            o.PaymentIntentId,
            o.PaymentStatus,
            o.CreatedAt,
            o.Appointment,
            o.EventId);
    }
}