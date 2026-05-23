using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repositoryMock = new();
    private readonly SseConnectionManager _sseConnectionManager = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_repositoryMock.Object, _sseConnectionManager);
    }

    [Fact]
    public async Task SendAsync_PersistsNotification()
    {
        var userId = Guid.NewGuid();

        await _sut.SendAsync(userId, "ORDER_PLACED", "Order placed", "Your order was placed.");

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Notification>(n =>
                    n.UserId == userId &&
                    n.Type == NotificationType.ORDER_PLACED &&
                    n.Title == "Order placed" &&
                    n.Message == "Your order was placed."),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenUserConnected_WritesSsePayload()
    {
        var userId = Guid.NewGuid();
        await using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;
        _sseConnectionManager.Add(userId, httpContext.Response);

        await _sut.SendAsync(userId, "PAYMENT_ACCEPTED", "Payment accepted", "Paid.");

        body.Position = 0;
        using var reader = new StreamReader(body);
        var payload = await reader.ReadToEndAsync();

        Assert.StartsWith("data: ", payload);
        Assert.Contains("\"UserId\":\"" + userId, payload);
        Assert.Contains("\"Type\":5", payload);
        Assert.EndsWith("\n\n", payload);
    }

    [Fact]
    public async Task SendBulkAsync_PersistsAllNotificationsAndPushesConnectedUsers()
    {
        var connectedUserId = Guid.NewGuid();
        var disconnectedUserId = Guid.NewGuid();
        await using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;
        _sseConnectionManager.Add(connectedUserId, httpContext.Response);

        await _sut.SendBulkAsync(
            new[]
            {
                (connectedUserId, "EVENT_STATUS_UPDATED", "Event updated", "Approved."),
                (disconnectedUserId, "ORDER_REJECTED", "Order rejected", "Rejected.")
            });

        _repositoryMock.Verify(
            x => x.AddRangeAsync(
                It.Is<IEnumerable<Notification>>(items =>
                    items.Count() == 2 &&
                    items.Any(n => n.UserId == connectedUserId && n.Type == NotificationType.EVENT_STATUS_UPDATED) &&
                    items.Any(n => n.UserId == disconnectedUserId && n.Type == NotificationType.ORDER_REJECTED)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        body.Position = 0;
        using var reader = new StreamReader(body);
        var payload = await reader.ReadToEndAsync();

        Assert.Contains("Event updated", payload);
        Assert.DoesNotContain("Order rejected", payload);
    }
}
