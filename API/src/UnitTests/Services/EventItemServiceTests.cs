using Application.DTOs;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class EventItemServiceTests
{
    private readonly Mock<IEventItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IEventRepository> _eventRepositoryMock = new();
    private readonly EventItemService _sut;

    public EventItemServiceTests()
    {
        _sut = new EventItemService(_itemRepositoryMock.Object, _eventRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventItem)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
    {
        var item = Item();
        _itemRepositoryMock
            .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _sut.GetByIdAsync(item.Id, CancellationToken.None);

        Assert.Equal(item.Id, result.Id);
        Assert.Equal(item.EventId, result.EventId);
        Assert.Equal("Decor", result.ServiceName);
        Assert.Equal(item.Quantity, result.Quantity);
    }

    [Fact]
    public async Task GetByEventIdAsync_WhenEventMissing_ThrowsKeyNotFoundException()
    {
        _eventRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByEventIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetByEventIdAsync_WhenEventExists_ReturnsItems()
    {
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _itemRepositoryMock.Setup(x => x.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync([Item(eventId: eventId)]);

        var result = (await _sut.GetByEventIdAsync(eventId, CancellationToken.None)).ToList();

        Assert.Single(result);
        Assert.Equal(eventId, result[0].EventId);
    }

    [Fact]
    public async Task CreateAsync_WhenEventMissing_ThrowsKeyNotFoundException()
    {
        _eventRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(new CreateEventItemDto { EventId = Guid.NewGuid(), Quantity = 2 }, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenEventExists_CreatesAndReturnsItem()
    {
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _itemRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<EventItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventItem entity, CancellationToken _) =>
            {
                entity.Id = Guid.NewGuid();
                entity.Price = 25;
                entity.Service = new Service { Name = "Decor", ServiceImages = [] };
                return entity;
            });

        var result = await _sut.CreateAsync(new CreateEventItemDto { EventId = eventId, Quantity = 3 }, CancellationToken.None);

        Assert.Equal(eventId, result.EventId);
        Assert.Equal(3, result.Quantity);
        _itemRepositoryMock.Verify(x => x.CreateAsync(It.Is<EventItem>(i => i.EventId == eventId && i.Quantity == 3), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRangeAsync_SetsEventIdForEachDto()
    {
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _itemRepositoryMock
            .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<EventItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<EventItem> entities, CancellationToken _) => entities.Select(e =>
            {
                e.Id = Guid.NewGuid();
                e.Service = new Service { Name = "Decor", ServiceImages = [] };
                return e;
            }).ToList());

        var result = (await _sut.CreateRangeAsync(eventId, [new CreateEventItemDto { Quantity = 1 }, new CreateEventItemDto { Quantity = 2 }], CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal(eventId, item.EventId));
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((EventItem)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(Guid.NewGuid(), new UpdateEventItemDto { Quantity = 5 }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesQuantity()
    {
        var item = Item(quantity: 1);
        _itemRepositoryMock.Setup(x => x.GetByIdAsync(item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _itemRepositoryMock.Setup(x => x.UpdateAsync(item, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var result = await _sut.UpdateAsync(item.Id, new UpdateEventItemDto { Quantity = 5 }, CancellationToken.None);

        Assert.Equal(5, item.Quantity);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_Deletes()
    {
        var id = Guid.NewGuid();
        _itemRepositoryMock.Setup(x => x.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _itemRepositoryMock.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(id, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByEventIdAsync_WhenEventExists_DeletesItems()
    {
        var eventId = Guid.NewGuid();
        _eventRepositoryMock.Setup(x => x.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _itemRepositoryMock.Setup(x => x.DeleteByEventIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.DeleteByEventIdAsync(eventId, CancellationToken.None);

        Assert.True(result);
    }

    private static EventItem Item(Guid? eventId = null, int quantity = 2) => new()
    {
        Id = Guid.NewGuid(),
        EventId = eventId ?? Guid.NewGuid(),
        Price = 100,
        Quantity = quantity,
        Service = new Service { Id = Guid.NewGuid(), Name = "Decor", ServiceImages = [] }
    };
}
