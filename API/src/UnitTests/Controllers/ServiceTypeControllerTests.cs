using Application.DTOs.ServiceTypesDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Api.Controllers;
using Xunit;

namespace Application.UnitTests.Controllers;

public class ServiceTypeControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock = new();
    private readonly Mock<IServiceTypeService> _serviceTypeServiceMock = new();
    private readonly ServiceTypeController _sut;

    public ServiceTypeControllerTests()
    {
        _serviceManagerMock.SetupGet(x => x.ServiceTypeService).Returns(_serviceTypeServiceMock.Object);
        _sut = new ServiceTypeController(_serviceManagerMock.Object);
    }

    [Fact]
    public async Task GetServiceTypeById_WhenMissing_ReturnsNotFoundObjectResult()
    {
        _serviceTypeServiceMock
            .Setup(x => x.GetServiceTypeByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceTypeDTO>.NotFound(404, "Service type not found"));

        var result = await _sut.GetServiceTypeById(Guid.NewGuid(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task AddServiceType_WhenSuccess_ReturnsCreated()
    {
        var request = new CreateServiceTypeRequest { Name = "Decor", VendorTypeId = Guid.NewGuid() };
        _serviceTypeServiceMock.Setup(x => x.AddTypeAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceTypeDTO>.Success(new ServiceTypeDTO { Id = Guid.NewGuid(), Name = request.Name }));

        var result = await _sut.AddServiceType(request, CancellationToken.None);

        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task DeleteServiceType_WhenSuccess_ReturnsNoContent()
    {
        _serviceTypeServiceMock.Setup(x => x.DeleteTypeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceTypeDTO>.Success(new ServiceTypeDTO()));

        var result = await _sut.DeleteServiceType(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateServiceType_WhenSuccess_ReturnsNoContent()
    {
        _serviceTypeServiceMock.Setup(x => x.UpdateTypeAsync(It.IsAny<Guid>(), It.IsAny<UpdateServiceTypeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceTypeDTO>.Success(new ServiceTypeDTO()));

        var result = await _sut.UpdateServiceType(Guid.NewGuid(), new UpdateServiceTypeRequest { Name = "Decor" }, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
