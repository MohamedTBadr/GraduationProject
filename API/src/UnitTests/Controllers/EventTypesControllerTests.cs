using Application.DTOs.EventTypesDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Api.Controllers;
using Xunit;

namespace Application.UnitTests.Controllers;

public class EventTypesControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock = new();
    private readonly Mock<IEventTypeService> _eventTypeServiceMock = new();
    private readonly EventTypesController _sut;

    public EventTypesControllerTests()
    {
        _serviceManagerMock.SetupGet(x => x.EventTypeService).Returns(_eventTypeServiceMock.Object);
        _sut = new EventTypesController(_serviceManagerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEventTypes()
    {
        var items = new[] { new EventTypeResponseDto(Guid.NewGuid(), "Wedding") };
        _eventTypeServiceMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(items, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        _eventTypeServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((EventTypeResponseDto?)null);

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAndDelegatesToService()
    {
        var dto = new EventTypeCreateDto("Conference");
        _eventTypeServiceMock.Setup(x => x.CreateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new EventTypeResponseDto(Guid.NewGuid(), dto.Name));

        var result = await _sut.Create(dto, CancellationToken.None);

        Assert.IsType<CreatedResult>(result);
        _eventTypeServiceMock.Verify(x => x.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenRouteIdDoesNotMatchDto_ReturnsBadRequest()
    {
        var result = await _sut.Update(Guid.NewGuid(), new EventTypeUpdateDto(Guid.NewGuid(), "Updated"), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID mismatch", badRequest.Value);
    }

    [Fact]
    public async Task Delete_WhenServiceReturnsFalse_ReturnsNotFound()
    {
        _eventTypeServiceMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
