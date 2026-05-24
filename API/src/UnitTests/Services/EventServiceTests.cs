using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Application.Services;
using Application.DTOs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;

namespace Application.UnitTests.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepoMock;
        private readonly Mock<IEventTypeRepository> _eventTypeRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<INotificationRepository> _notificationRepoMock;
        private readonly NotificationService _notificationService;
        private readonly EventService _sut;

        public EventServiceTests()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventTypeRepoMock = new Mock<IEventTypeRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _emailSenderMock = new Mock<IEmailSender>();
            
            _notificationRepoMock = new Mock<INotificationRepository>();
            var sseManager = new SseConnectionManager();
            _notificationService = new NotificationService(_notificationRepoMock.Object, sseManager);

            _sut = new EventService(
                _eventRepoMock.Object,
                _eventTypeRepoMock.Object,
                _notificationService,
                _userRepoMock.Object,
                _emailSenderMock.Object);
        }

        [Fact]
        public async Task GetByStatusAsync_ValidStatus_ReturnsSuccessResultWithEntities()
        {
            // Arrange
            string validStatus = "Planned";
            var cancellationToken = CancellationToken.None;

            var events = new List<Event>
            {
                new Event { Id = Guid.NewGuid(), Title = "Event 1", EventStatus = validStatus, EventType = new EventType { Id = Guid.NewGuid(), Name = "Type 1" } },
                new Event { Id = Guid.NewGuid(), Title = "Event 2", EventStatus = validStatus, EventType = new EventType { Id = Guid.NewGuid(), Name = "Type 2" } }
            };

            _eventRepoMock.Setup(x => x.GetByStatusAsync(validStatus, cancellationToken))
                .ReturnsAsync(events);

            // Act
            var result = await _sut.GetByStatusAsync(validStatus, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
            _eventRepoMock.Verify(x => x.GetByStatusAsync(validStatus, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetByStatusAsync_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            string invalidStatus = "InvalidStatus";
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByStatusAsync(invalidStatus, cancellationToken));
            Assert.Contains("Invalid status", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_EventTypeNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var dto = new CreateEventDto { EventTypeId = Guid.NewGuid(), Title = "New Event" };
            var cancellationToken = CancellationToken.None;
            _eventTypeRepoMock.Setup(x => x.ExistsAsync(dto.EventTypeId, cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await _sut.CreateAsync(dto, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error?.Code);
        }

        [Fact]
        public async Task CreateAsync_ValidInput_ReturnsSuccessResult()
        {
            // Arrange
            var dto = new CreateEventDto { EventTypeId = Guid.NewGuid(), Title = "New Event", EventDate = DateTime.UtcNow, UserId = Guid.NewGuid() };
            var cancellationToken = CancellationToken.None;
            _eventTypeRepoMock.Setup(x => x.ExistsAsync(dto.EventTypeId, cancellationToken)).ReturnsAsync(true);
            
            var createdEvent = new Event { Id = Guid.NewGuid(), Title = dto.Title, EventStatus = "Planned", EventType = new EventType { Id = dto.EventTypeId, Name = "Type 1" } };
            _eventRepoMock.Setup(x => x.CreateAsync(It.IsAny<Event>(), cancellationToken)).ReturnsAsync(createdEvent);

            // Act
            var result = await _sut.CreateAsync(dto, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(createdEvent.Id, result.Value.Id);
            _eventRepoMock.Verify(x => x.CreateAsync(It.Is<Event>(e => e.Title == dto.Title), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_EventNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto { EventStatus = "Planned" };
            var cancellationToken = CancellationToken.None;
            _eventRepoMock.Setup(x => x.GetByIdWithItemsAsync(id, cancellationToken)).ReturnsAsync((Event)null!);

            // Act
            var result = await _sut.UpdateAsync(id, dto, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error?.Code);
        }

        [Fact]
        public async Task UpdateAsync_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto { EventStatus = "UnknownStatus" };
            var cancellationToken = CancellationToken.None;
            var existingEvent = new Event { Id = id, EventStatus = "Planned" };
            _eventRepoMock.Setup(x => x.GetByIdWithItemsAsync(id, cancellationToken)).ReturnsAsync(existingEvent);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(id, dto, cancellationToken));
        }

        [Fact]
        public async Task UpdateAsync_ValidInputNotCompleting_UpdatesAndReturnsSuccessWithoutEmail()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto { Title = "Updated", EventStatus = "Approved" };
            var cancellationToken = CancellationToken.None;
            
            var existingEvent = new Event { Id = id, Title = "Old", EventStatus = "Planned", EventType = new EventType { Id = Guid.NewGuid(), Name = "Type 1" } };
            _eventRepoMock.Setup(x => x.GetByIdWithItemsAsync(id, cancellationToken)).ReturnsAsync(existingEvent);
            _eventRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Event>(), cancellationToken)).ReturnsAsync(existingEvent);

            // Act
            var result = await _sut.UpdateAsync(id, dto, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Updated", existingEvent.Title);
            Assert.Equal("Approved", existingEvent.EventStatus);
            _eventRepoMock.Verify(x => x.UpdateAsync(existingEvent, cancellationToken), Times.Once);
            _emailSenderMock.Verify(x => x.SendCongratulatoryEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidInputCompleting_UpdatesAndReturnsSuccessWithEmail()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto { Title = "Updated", EventStatus = "Completed" };
            var cancellationToken = CancellationToken.None;
            
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.com" };
            var existingEvent = new Event 
            { 
                Id = id, 
                Title = "Test Event", 
                EventStatus = "Approved",
                UserId = user.Id,
                User = user,
                EventType = new EventType { Id = Guid.NewGuid(), Name = "Type 1" } 
            };
            
            _eventRepoMock.Setup(x => x.GetByIdWithItemsAsync(id, cancellationToken)).ReturnsAsync(existingEvent);
            _eventRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Event>(), cancellationToken)).ReturnsAsync(existingEvent);
            _userRepoMock.Setup(x => x.GetByIdAsync(user.Id, cancellationToken)).ReturnsAsync(user);

            // Act
            var result = await _sut.UpdateAsync(id, dto, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Completed", existingEvent.EventStatus);
            _eventRepoMock.Verify(x => x.UpdateAsync(existingEvent, cancellationToken), Times.Once);
            _emailSenderMock.Verify(x => x.SendCongratulatoryEmailAsync(user.Email, user.FirstName, existingEvent.Title), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_EventNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            _eventRepoMock.Setup(x => x.ExistsAsync(id, cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteAsync(id, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error?.Code);
        }

        [Fact]
        public async Task DeleteAsync_EventExists_ReturnsSuccessResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;
            _eventRepoMock.Setup(x => x.ExistsAsync(id, cancellationToken)).ReturnsAsync(true);
            _eventRepoMock.Setup(x => x.DeleteAsync(id, cancellationToken)).ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteAsync(id, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            _eventRepoMock.Verify(x => x.DeleteAsync(id, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_InvalidStatus_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var invalidStatus = "Invalid";
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateStatusAsync(id, invalidStatus, cancellationToken));
        }

        [Fact]
        public async Task UpdateStatusAsync_EventNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var status = "Completed";
            var cancellationToken = CancellationToken.None;
            _eventRepoMock.Setup(x => x.GetByIdAsync(id, cancellationToken)).ReturnsAsync((Event)null!);

            // Act
            var result = await _sut.UpdateStatusAsync(id, status, cancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error?.Code);
        }

        [Fact]
        public async Task UpdateStatusAsync_EventExistsNotCompleting_UpdatesStatusSendsNotification()
        {
            // Arrange
            var id = Guid.NewGuid();
            var status = "Approved";
            var cancellationToken = CancellationToken.None;
            var existingEvent = new Event { Id = id, EventStatus = "Planned", UserId = Guid.NewGuid() };
            _eventRepoMock.Setup(x => x.GetByIdAsync(id, cancellationToken)).ReturnsAsync(existingEvent);

            // Act
            var result = await _sut.UpdateStatusAsync(id, status, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            Assert.Equal(status, existingEvent.EventStatus);
            _eventRepoMock.Verify(x => x.UpdateAsync(existingEvent, cancellationToken), Times.Once);
            _notificationRepoMock.Verify(x => x.AddAsync(It.Is<Notification>(n => n.UserId == existingEvent.UserId && n.Title == "Event Status Updated"), It.IsAny<CancellationToken>()), Times.Once);
            _emailSenderMock.Verify(x => x.SendCongratulatoryEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_EventExistsCompleting_UpdatesStatusSendsNotificationAndEmail()
        {
            // Arrange
            var id = Guid.NewGuid();
            var status = "Completed";
            var cancellationToken = CancellationToken.None;
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com" };
            var existingEvent = new Event { Id = id, EventStatus = "Approved", UserId = user.Id, User = user };
            _eventRepoMock.Setup(x => x.GetByIdAsync(id, cancellationToken)).ReturnsAsync(existingEvent);
            _userRepoMock.Setup(x => x.GetByIdAsync(user.Id, cancellationToken)).ReturnsAsync(user);

            // Act
            var result = await _sut.UpdateStatusAsync(id, status, cancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            Assert.Equal(status, existingEvent.EventStatus);
            _eventRepoMock.Verify(x => x.UpdateAsync(existingEvent, cancellationToken), Times.Once);
            _notificationRepoMock.Verify(x => x.AddAsync(It.Is<Notification>(n => n.UserId == existingEvent.UserId && n.Title == "Event Status Updated"), It.IsAny<CancellationToken>()), Times.Once);
            _emailSenderMock.Verify(x => x.SendCongratulatoryEmailAsync(user.Email, user.FirstName, existingEvent.Title), Times.Once);
        }
    }
}