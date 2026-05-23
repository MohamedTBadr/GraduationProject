using Application.Hubs;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class ChatServiceTests
{
    private readonly Mock<IMessageRepository> _repositoryMock = new();
    private readonly Mock<IChatNotificationService> _notificationMock = new();
    private readonly ChatService _sut;

    public ChatServiceTests()
    {
        _sut = new ChatService(_repositoryMock.Object, _notificationMock.Object, Mock.Of<ILogger<ChatService>>());
    }

    [Fact]
    public async Task SendMessageAsync_CreatesMessageSavesAndNotifiesReceiver()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetOrCreateConversationAsync(senderId, receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Conversation.Create(senderId, receiverId));

        var result = await _sut.SendMessageAsync(senderId, receiverId, "Hello", CancellationToken.None);

        Assert.Equal(senderId, result.SenderId);
        Assert.Equal(receiverId, result.ReceiverId);
        Assert.Equal("Hello", result.Content);
        Assert.False(result.IsRead);
        _repositoryMock.Verify(x => x.AddMessageAsync(It.Is<Message>(m => m.Content == "Hello"), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationMock.Verify(x => x.SendMessageAsync(receiverId.ToString(), It.IsAny<Application.DTOs.MessageDTOs.MessageDto>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_EmptyContent_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SendMessageAsync(Guid.NewGuid(), Guid.NewGuid(), " ", CancellationToken.None));
    }

    [Fact]
    public async Task GetMessagesAsync_WhenConversationMissing_ReturnsEmpty()
    {
        _repositoryMock
            .Setup(x => x.GetConversationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var result = await _sut.GetMessagesAsync(Guid.NewGuid(), Guid.NewGuid(), 1, 20, CancellationToken.None);

        Assert.Empty(result);
        _repositoryMock.Verify(x => x.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenMessageMissing_DoesNothing()
    {
        _repositoryMock
            .Setup(x => x.GetMessageByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        await _sut.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notificationMock.Verify(x => x.NotifyMessageReadAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenReaderIsReceiver_MarksReadAndNotifiesSender()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var message = Message.Create(senderId, receiverId, "Hello");
        _repositoryMock
            .Setup(x => x.GetMessageByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        await _sut.MarkAsReadAsync(message.Id, receiverId, CancellationToken.None);

        Assert.True(message.IsRead);
        Assert.NotNull(message.ReadAt);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationMock.Verify(x => x.NotifyMessageReadAsync(senderId.ToString(), message.Id), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenReaderIsNotReceiver_DoesNothing()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");
        _repositoryMock
            .Setup(x => x.GetMessageByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        await _sut.MarkAsReadAsync(message.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.False(message.IsRead);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
