using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Application.UnitTests.Controllers;

public class VendorTypeControllerTests
{
    private readonly Mock<IServiceManager> _serviceManagerMock = new();
    private readonly Mock<IVendorTypeService> _vendorTypeServiceMock = new();
    private readonly VendorTypeController _sut;

    public VendorTypeControllerTests()
    {
        _serviceManagerMock.SetupGet(x => x.VendorTypeService).Returns(_vendorTypeServiceMock.Object);
        _sut = new VendorTypeController(_serviceManagerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccess_ReturnsOk()
    {
        var resultValue = Result<IReadOnlyList<VendorTypeDetailsDTO>>.Success([new VendorTypeDetailsDTO { Id = Guid.NewGuid(), Name = "Venue" }]);
        _vendorTypeServiceMock.Setup(x => x.GetVendorTypesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(resultValue);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(resultValue, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFoundObjectResult()
    {
        _vendorTypeServiceMock
            .Setup(x => x.GetVendorTypeByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VendorTypeDetailsDTO>.NotFound(404, "Vendor type not found"));

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task Create_WhenModelStateInvalid_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Name", "Required");

        var result = await _sut.Create(new CreateOrUpdateVendorTypeRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _vendorTypeServiceMock.Verify(x => x.AddVendorTypeAsync(It.IsAny<CreateOrUpdateVendorTypeRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new CreateOrUpdateVendorTypeRequest { Name = "Decor" };
        var serviceResult = Result<VendorTypeDetailsDTO>.Success(new VendorTypeDetailsDTO { Id = id, Name = request.Name });
        _vendorTypeServiceMock.Setup(x => x.UpdateVendorTypeAsync(id, request, It.IsAny<CancellationToken>())).ReturnsAsync(serviceResult);

        var result = await _sut.Update(id, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(serviceResult, ok.Value);
    }

    [Fact]
    public async Task Delete_WhenSuccess_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var serviceResult = Result<bool>.Success(true);
        _vendorTypeServiceMock.Setup(x => x.DeleteVendorTypeAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(serviceResult);

        var result = await _sut.Delete(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(serviceResult, ok.Value);
    }
}
