using Application.DTOs.Orders;
using Application.DTOs.Vouchers;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IVoucherService> _voucherServiceMock = new();
        private readonly Mock<INotificationRepository> _notifRepoMock = new();

        private readonly NotificationService _notificationService;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            var sseManager = new SseConnectionManager();

            _notificationService = new NotificationService(
                _notifRepoMock.Object,
                sseManager);

            _orderService = new OrderService(
                _orderRepoMock.Object,
                _notificationService,
                _voucherServiceMock.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_NullShippingAddress_ReturnsFailureResult()
        {
            var request = new CreateOrderRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "EGP",
                null!,
                null,
                null);

            var result = await _orderService.CreateOrderAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Equal(400, result.Error.Code);
            Assert.Equal("Shipping address is required.", result.Error.Description);
        }

        [Fact]
        public async Task CreateOrderAsync_EventMissing_ThrowsKeyNotFoundException()
        {
            var request = ValidCreateOrderRequest();

            _orderRepoMock
                .Setup(x => x.GetEventWithItemsAsync(
                    request.EventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var result = await _orderService.CreateOrderAsync(request, CancellationToken.None);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Equal(404, result.Error.Code);
        }

        [Fact]
        public async Task CreateOrderAsync_EventBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
        {
            var request = ValidCreateOrderRequest();

            _orderRepoMock
                .Setup(x => x.GetEventWithItemsAsync(
                    request.EventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event
                {
                    Id = request.EventId,
                    UserId = Guid.NewGuid()
                });

            var result = await _orderService.CreateOrderAsync(request, CancellationToken.None);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Equal(403, result.Error?.Code);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequest_CalculatesAmountPersistsOrderAndNotifies()
        {
            var vendorId = Guid.NewGuid();

            var request = ValidCreateOrderRequest();

            _orderRepoMock
                .Setup(x => x.GetEventWithItemsAsync(
                    request.EventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event
                {
                    Id = request.EventId,
                    UserId = request.UserId,
                    EventItems =
                    [
                        new EventItem
                        {
                            Quantity = 2,
                            Price = 100,
                            Service = new Service
                            {
                                Name = "Decor",
                                VendorId = vendorId
                            }
                        },
                        new EventItem
                        {
                            Quantity = 1,
                            Price = 50,
                            Service = new Service
                            {
                                Name = "Lighting",
                                VendorId = vendorId
                            }
                        }
                    ]
                });

            var result = await _orderService.CreateOrderAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.Equal(250, result.Value.Amount);
            Assert.Equal("EGP", result.Value.Currency);
            Assert.Equal(request.EventId, result.Value.EventId);

            _orderRepoMock.Verify(
                x => x.AddAsync(
                    It.Is<Order>(o =>
                        o.UserId == request.UserId &&
                        o.EventId == request.EventId &&
                        o.Amount == 250 &&
                        o.ShippingAddress.City == request.ShippingAddress.City),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _notifRepoMock.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(n =>
                        n.UserId == request.UserId &&
                        n.Type == NotificationType.ORDER_PLACED),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _notifRepoMock.Verify(
                x => x.AddRangeAsync(
                    It.Is<IEnumerable<Notification>>(items =>
                        items.Single().UserId == vendorId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidVoucher_AppliesDiscountAndMarksVoucherUsed()
        {
            var request = ValidCreateOrderRequest("SAVE10");

            _orderRepoMock
                .Setup(x => x.GetEventWithItemsAsync(
                    request.EventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event
                {
                    Id = request.EventId,
                    UserId = request.UserId,
                    EventItems =
                    [
                        new EventItem
                {
                    Quantity = 2,
                    Price = 100,
                    Service = new Service
                    {
                        Name = "Decor",
                        VendorId = Guid.NewGuid()
                    }
                }
                    ]
                });

            _voucherServiceMock
                .Setup(x => x.ValidateVoucherAsync(
                    "SAVE10",
                    request.UserId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result<ApplyVoucherResult>.Success(
                        new ApplyVoucherResult(true, 10, null)));

            var result = await _orderService.CreateOrderAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.NotNull(result.Value);

            // 200 - 10% = 180
            Assert.Equal(180, result.Value.Amount);

            _voucherServiceMock.Verify(
                x => x.MarkVoucherUsedAsync(
                    "SAVE10",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
      
        [Fact]
        public async Task GetOrderByIdAsync_WhenMissing_ReturnsFailure()
        {
            _orderRepoMock
                .Setup(x => x.GetByIdWithItemsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var result = await _orderService.GetOrderByIdAsync(
                Guid.NewGuid(),
                CancellationToken.None);

            Assert.False(result.IsSuccess);

            Assert.NotNull(result.Error);

            Assert.Equal(404, result.Error.Code);
        }
        [Fact]
        public async Task GetAllOrdersAsync_ReturnsMappedOrders()
        {
            _orderRepoMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        Amount = 25,
                        EventId = Guid.NewGuid()
                    }
                ]);

            var result = (await _orderService
                    .GetAllOrdersAsync(CancellationToken.None))
                    .Value
                    .ToList();

            Assert.Single(result);

            Assert.Equal(25, result[0].Amount);
        }

        [Fact]
        public async Task UpdatePaymentStatusAsync_SameStatus_ReturnsWithoutUpdating()
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PaymentStatus = "Paid",
                EventId = Guid.NewGuid()
            };

            _orderRepoMock
                .Setup(x => x.GetByIdWithItemsAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var result = await _orderService.UpdatePaymentStatusAsync(
                order.Id,
                new UpdateOrderStatusRequest("paid"),
                CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.Equal("Paid", result.Value.PaymentStatus);

            _orderRepoMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData("Paid", NotificationType.PAYMENT_ACCEPTED)]
        [InlineData("Failed", NotificationType.PAYMENT_REJECTED)]
        [InlineData("Completed", NotificationType.ORDER_COMPLETED)]
        [InlineData("Rejected", NotificationType.ORDER_REJECTED)]
        public async Task UpdatePaymentStatusAsync_KnownStatus_UpdatesAndSendsNotification(
            string status,
            NotificationType expectedType)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PaymentStatus = "Pending",
                VoucherCode = "SAVE",
                EventId = Guid.NewGuid()
            };

            _orderRepoMock
                .Setup(x => x.GetByIdWithItemsAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var result = await _orderService.UpdatePaymentStatusAsync(
                order.Id,
                new UpdateOrderStatusRequest(status),
                CancellationToken.None);

            Assert.Equal(status, result.Value.PaymentStatus);

            _orderRepoMock.Verify(
                x => x.UpdateAsync(
                    order,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _notifRepoMock.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(n => n.Type == expectedType),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData("Failed")]
        [InlineData("Rejected")]
        public async Task UpdatePaymentStatusAsync_FailedOrRejectedWithVoucher_MarksVoucherUnused(
            string status)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PaymentStatus = "Pending",
                VoucherCode = "SAVE",
                EventId = Guid.NewGuid()
            };

            _orderRepoMock
                .Setup(x => x.GetByIdWithItemsAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            await _orderService.UpdatePaymentStatusAsync(
                order.Id,
                new UpdateOrderStatusRequest(status),
                CancellationToken.None);

            _voucherServiceMock.Verify(
                x => x.MarkVoucherUnusedAsync(
                    "SAVE",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SetPaymentIntentAsync_UpdatesIntentAndReturnsOrder()
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventId = Guid.NewGuid()
            };

            _orderRepoMock
                .Setup(x => x.GetByIdWithItemsAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var result = await _orderService.SetPaymentIntentAsync(
                order.Id,
                "intent-123",
                CancellationToken.None);

            Assert.Equal("intent-123", result.Value.PaymentIntentId);

            _orderRepoMock.Verify(
                x => x.UpdateAsync(
                    order,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_WhenAlreadyCancelled_DoesNothing()
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                PaymentStatus = "Cancelled"
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            await _orderService.CancelOrderAsync(
                order.Id,
                CancellationToken.None);

            _orderRepoMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_WhenActive_CancelsRevertsVoucherAndNotifies()
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                PaymentStatus = "Pending",
                VoucherCode = "SAVE",
            };

            _orderRepoMock
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            await _orderService.CancelOrderAsync(
                order.Id,
                CancellationToken.None);

            Assert.Equal("Cancelled", order.PaymentStatus);

            _voucherServiceMock.Verify(
                x => x.MarkVoucherUnusedAsync(
                    "SAVE",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _notifRepoMock.Verify(
                x => x.AddAsync(
                    It.Is<Notification>(n =>
                        n.Type == NotificationType.ORDER_CANCELLED),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task CreateOrderAsync_InvalidVoucher_ReturnsInvalidOperationResult()
        {
            var request = ValidCreateOrderRequest("BAD");

            _orderRepoMock
                .Setup(x => x.GetEventWithItemsAsync(request.EventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = request.EventId, UserId = request.UserId });

            _voucherServiceMock
                .Setup(x => x.ValidateVoucherAsync("BAD", request.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ApplyVoucherResult>.Success(new ApplyVoucherResult(IsValid: false, DiscountPercent: 0, "BAD")));
            var result = await _orderService.CreateOrderAsync(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(400, result.Error?.Code);
            Assert.Equal(ErrorType.InvalidOperation, result.Error?.Type);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenMissing_ReturnsNotFoundResult()
        {
            _orderRepoMock
                .Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _orderService.DeleteOrderAsync(Guid.NewGuid(), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(404, result.Error?.Code);
            Assert.Equal(ErrorType.NotFound, result.Error?.Type);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenExists_Deletes()
        {
            var id = Guid.NewGuid();

            _orderRepoMock
                .Setup(x => x.ExistsAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _orderService.DeleteOrderAsync(
                id,
                CancellationToken.None);

            _orderRepoMock.Verify(
                x => x.DeleteAsync(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static CreateOrderRequest ValidCreateOrderRequest(
        string? voucherCode = null)
        => new(
            Guid.NewGuid(), // UserId
            Guid.NewGuid(), // EventId
            "EGP",
            new AddressDto(
                "Street 1",
                "Cairo",
                "Cairo",
                "12345"),
            DateTime.UtcNow.AddDays(1),
            voucherCode);

    }
}
