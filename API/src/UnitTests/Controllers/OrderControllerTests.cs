using API.Controllers;
using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Application.UnitTests.Controllers;

public class OrderControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock = new();
    private readonly Mock<IOrderService> _orderServiceMock = new();
    private readonly OrderController _sut;

    public OrderControllerTests()
    {
        _serviceManagerMock.SetupGet(x => x.OrderService).Returns(_orderServiceMock.Object);
        _sut = new OrderController(_serviceManagerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithOrders()
    {
        var orders = new[] { OrderResponse(Guid.NewGuid()) };
        _orderServiceMock.Setup(x => x.GetAllOrdersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(orders, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("missing"));
        SetUser(Guid.NewGuid(), "Admin");

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenUserIsNotOwner_ReturnsForbid()
    {
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(OrderResponse(Guid.NewGuid()));
        SetUser(Guid.NewGuid(), "Customer");

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetById_WhenAdmin_ReturnsOk()
    {
        var ownerId = Guid.NewGuid();
        var order = OrderResponse(ownerId);
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        SetUser(Guid.NewGuid(), "Admin");

        var result = await _sut.GetById(order.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(order, ok.Value);
    }

    [Fact]
    public async Task GetByUser_WhenNotOwnerOrAdmin_ReturnsForbid()
    {
        SetUser(Guid.NewGuid(), "Customer");

        var result = await _sut.GetByUser(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdatePaymentStatus_WhenServiceThrowsKeyNotFound_ReturnsNotFound()
    {
        _orderServiceMock.Setup(x => x.UpdatePaymentStatusAsync(It.IsAny<Guid>(), It.IsAny<UpdateOrderStatusRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("missing"));

        var result = await _sut.UpdatePaymentStatus(Guid.NewGuid(), new UpdateOrderStatusRequest("Paid"), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Cancel_WhenCustomerCancelsPaidOrder_ReturnsBadRequest()
    {
        var ownerId = Guid.NewGuid();
        var order = OrderResponse(ownerId) with { PaymentStatus = "Paid" };
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        SetUser(ownerId, "Customer");

        var result = await _sut.Cancel(order.Id, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _orderServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenServiceSucceeds_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        var result = await _sut.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _orderServiceMock.Verify(x => x.DeleteOrderAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetUser(Guid userId, string role)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            ],
            "TestAuth");

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static OrderResponse OrderResponse(Guid userId) => new(
        Guid.NewGuid(),
        userId,
        100,
        "EGP",
        null,
        "Pending",
        DateTime.UtcNow,
        null,
        Guid.NewGuid());
}
