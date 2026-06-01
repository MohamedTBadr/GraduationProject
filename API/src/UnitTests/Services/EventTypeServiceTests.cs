using Application.DTOs.EventTypesDTOs;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class EventTypeServiceTests
{
    private readonly Mock<IEventTypeRepository> _repositoryMock = new();
    private readonly EventTypeService _sut;

    public EventTypeServiceTests()
    {
        _sut = new EventTypeService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new EventType { Id = Guid.NewGuid(), Name = "Wedding" }]);

        var result = (await _sut.GetAllAsync(CancellationToken.None)).Value.ToList();

        Assert.Single(result);
        Assert.Equal("Wedding", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsFailure()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventType)null!);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.Error!.Code);
    }

    [Fact]
    public async Task CreateAsync_PersistsEntityAndReturnsDto()
    {
        var result = await _sut.CreateAsync(new EventTypeCreateDto("Conference"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal("Conference", result.Value.Name);
        _repositoryMock.Verify(
            x => x.CreateAsync(It.Is<EventType>(e => e.Id == result.Value.Id && e.Name == "Conference"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityExists_UpdatesName()
    {
        var existing = new EventType { Id = Guid.NewGuid(), Name = "Old" };
        _repositoryMock
            .Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.UpdateAsync(new EventTypeUpdateDto(existing.Id, "New"), CancellationToken.None);

        Assert.Equal("New", existing.Name);
        _repositoryMock.Verify(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventType)null!);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<EventType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        var existing = new EventType { Id = Guid.NewGuid(), Name = "Wedding" };
        _repositoryMock
            .Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.DeleteAsync(existing.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }
}
