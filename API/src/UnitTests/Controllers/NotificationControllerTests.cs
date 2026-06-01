using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Application.UnitTests.Controllers;

public class NotificationControllerTests
{
    private readonly Mock<INotificationRepository> _repoMock;
    private readonly SseConnectionManager _sseManager;
    private readonly NotificationService _notificationService;
    private readonly NotificationsController _sut;

    public NotificationControllerTests()
    {
        _repoMock = new Mock<INotificationRepository>();
        _sseManager = new SseConnectionManager();
        _notificationService = new NotificationService(_repoMock.Object, _sseManager);
        
        _sut = new NotificationsController(_notificationService, _repoMock.Object, _sseManager);
    }

    [Fact]
    public async Task Stream_WhenCalled_AddsAndRemovesConnectionAndSetsHeaders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = user
        };
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately so Task.Delay throws TaskCanceledException

        // Act
        await _sut.Stream(cts.Token);

        // Assert
        Assert.Equal("text/event-stream", httpContext.Response.Headers["Content-Type"]);
        Assert.Equal("no-cache", httpContext.Response.Headers["Cache-Control"]);
        Assert.Equal("keep-alive", httpContext.Response.Headers["Connection"]);
        
        // sseManager should be empty because we removed it in finally block
        Assert.False(_sseManager.TryGet(userId, out _));
    }

    [Fact]
    public async Task GetAll_WhenCalled_ReturnsOkWithNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = user };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var expectedNotifications = new List<Notification> { new Notification { Id = Guid.NewGuid() } };
        _repoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotifications);

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualNotifications = Assert.IsAssignableFrom<List<Notification>>(okResult.Value);
        Assert.Equal(expectedNotifications, actualNotifications);
    }

    [Fact]
    public async Task MarkRead_WhenCalled_CallsRepoAndReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = user };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var notificationId = Guid.NewGuid();

        _repoMock.Setup(x => x.MarkAsReadAsync(notificationId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.MarkRead(notificationId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _repoMock.Verify(x => x.MarkAsReadAsync(notificationId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
