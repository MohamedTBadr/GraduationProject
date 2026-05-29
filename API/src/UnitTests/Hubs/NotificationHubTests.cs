using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Common.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Application.UnitTests.Hubs
{
    public class NotificationHubTests
    {
        [Fact]
        public async Task OnConnectedAsync_WithValidUserId_AddsToGroup()
        {
            // Arrange
            var hub = new NotificationHub();
            
            var connectionId = "connection-123";
            var userId = "user-123";
            
            var claims = new[] { new Claim("id", userId) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            
            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
            mockContext.Setup(c => c.User).Returns(principal);
            
            var mockGroupManager = new Mock<IGroupManager>();
            
            hub.Context = mockContext.Object;
            hub.Groups = mockGroupManager.Object;
            
            // Act
            await hub.OnConnectedAsync();
            
            // Assert
            mockGroupManager.Verify(g => g.AddToGroupAsync(connectionId, userId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task OnConnectedAsync_WithNullUserId_DoesNotAddToGroup()
        {
            // Arrange
            var hub = new NotificationHub();
            
            var connectionId = "connection-123";
            
            var claims = new Claim[0]; // No id claim
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            
            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
            mockContext.Setup(c => c.User).Returns(principal);
            
            var mockGroupManager = new Mock<IGroupManager>();
            
            hub.Context = mockContext.Object;
            hub.Groups = mockGroupManager.Object;
            
            // Act
            await hub.OnConnectedAsync();
            
            // Assert
            mockGroupManager.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task OnConnectedAsync_WithNullUser_DoesNotAddToGroup()
        {
            // Arrange
            var hub = new NotificationHub();
            
            var connectionId = "connection-123";
            
            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
            mockContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);
            
            var mockGroupManager = new Mock<IGroupManager>();
            
            hub.Context = mockContext.Object;
            hub.Groups = mockGroupManager.Object;
            
            // Act
            await hub.OnConnectedAsync();
            
            // Assert
            mockGroupManager.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}