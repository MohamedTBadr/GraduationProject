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

        public async Task<Result<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct )
        {
            if (request.ShippingAddress is null)
                return Result<OrderResponse>.Validation(400, "Shipping address is required.");

            // 1. Fetch event and validate ownership (IDOR Fix)
            var eventWithItems = await orderRepo.GetEventWithItemsAsync(request.EventId, ct);
                
            if(eventWithItems is null)
                return Result<OrderResponse>.NotFound(404, $"Event {request.EventId} was not found.");

            if (eventWithItems.UserId != request.UserId)
                return Result<OrderResponse>.Unauthorized(403, "You are not authorized to create an order for this event.");

            // 2. Calculate base amount in-memory (Performance Optimization)
            var amount = eventWithItems.EventItems?.Sum(ei => ei.Quantity * ei.Price) ?? 0;

            // 3. Apply voucher discount if provided
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var voucherResult = await voucherService.ValidateVoucherAsync(
                    request.VoucherCode, request.UserId, ct);

                if (!voucherResult.Value.IsValid)
                    return Result<OrderResponse>.InvalidOperation(400, "Invalid voucher code.");

                var discount = amount * (voucherResult.Value.DiscountPercent / 100);
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
                        .GroupBy(i => i.Service.VendorId)
                        .Select(g => (
                            UserId: g.Key,
                            Type: nameof(NotificationType.ORDER_PLACED),
                            Title: "Order Details",
                            Message: $"Order #{order.Id} details: {string.Join(", ", g.Select(i => $"{i.Quantity}x {i.Service.Name}"))}."))
                        .ToList();

                    await notificationService.SendBulkAsync(vendorNotifications);
                }
            }

           var result = MapToResponse(order);
           return Result<OrderResponse>.Success(result);
        }

        // ─── Read ─────────────────────────────────────────────────────────────────

        public async Task<Result<OrderResponse>> GetOrderByIdAsync(Guid id, CancellationToken ct )
        {
            var order = await orderRepo.GetByIdWithItemsAsync(id, ct);
                if (order == null)
            { return Result<OrderResponse>.NotFound(404, $"Order {id} not found."); }

            return Result<OrderResponse>.Success(MapToResponse(order));
        }

        public async Task<Result<IEnumerable<OrderResponse>>> GetAllOrdersAsync(CancellationToken ct )
        {
            var orders = await orderRepo.GetAllAsync(ct);
            return Result<IEnumerable<OrderResponse>>.Success(orders.Select(MapToResponse));
        }

        public async Task<Result<IEnumerable<OrderResponse>>> GetOrdersByUserIdAsync(Guid userId, CancellationToken ct )
        {
            var orders = await orderRepo.GetByUserIdAsync(userId, ct);
            return Result<IEnumerable<OrderResponse>>.Success(orders.Select(MapToResponse));
        }

        // ─── Update ───────────────────────────────────────────────────────────────

        public async Task<Result<OrderResponse>> UpdatePaymentStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct )
        {
            var order = await orderRepo.GetByIdWithItemsAsync(id, ct);
            if (order is null)
                return Result<OrderResponse>.NotFound(404, $"Order {id} not found.");

            // Idempotency check: if status is already the same, skip processing
            if (order.PaymentStatus.Equals(request.PaymentStatus, StringComparison.OrdinalIgnoreCase))
                return Result<OrderResponse>.Success(MapToResponse(order));

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

            return Result<OrderResponse>.Success(MapToResponse(order));
        }

        public async Task<Result<OrderResponse>> SetPaymentIntentAsync(Guid id, string paymentIntentId, CancellationToken ct )
        {
            var order = await orderRepo.GetByIdWithItemsAsync(id, ct);



                if (order == null) {
                return Result<OrderResponse>.NotFound(404, $"Order {id} not found.");
            }
            order.PaymentIntentId = paymentIntentId;
            await orderRepo.UpdateAsync(order, ct);
            return Result<OrderResponse>.Success(MapToResponse(order));
        }

        // ─── Cancel / Delete ──────────────────────────────────────────────────────

        public async Task<Result<bool>> CancelOrderAsync(Guid id, CancellationToken ct)
        {
            var order = await orderRepo.GetByIdAsync(id, ct);
            if (order is null)
                return Result<bool>.NotFound(404, $"Order {id} not found.");

            if (order.PaymentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return Result<bool>.Success(true);

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

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteOrderAsync(Guid id, CancellationToken ct )
        {
            if (!await orderRepo.ExistsAsync(id, ct))
                return Result<bool>.NotFound(404, $"Order {id} not found.");

            await orderRepo.DeleteAsync(id, ct);
            return Result<bool>.Success(true);
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
