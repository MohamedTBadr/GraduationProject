using Application.DTOs;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class VendorTypeServiceTests
{
    private readonly Mock<IVendorTypeRepository> _repositoryMock = new();
    private readonly VendorTypeService _sut;

    public VendorTypeServiceTests()
    {
        _sut = new VendorTypeService(_repositoryMock.Object);
    }

    [Fact]
    public async Task AddVendorTypeAsync_PersistsEntityAndReturnsDto()
    {
        var request = new CreateOrUpdateVendorTypeRequest { Name = "Photography" };

        var result = await _sut.AddVendorTypeAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.Name, result.Value.Name);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        _repositoryMock.Verify(
            x => x.AddVendorTypeAsync(
                It.Is<VendorType>(v => v.Id == result.Value.Id && v.Name == request.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetVendorTypeByIdAsync_WhenMissing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetVendorTypeByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorType?)null);

        var result = await _sut.GetVendorTypeByIdAsync(id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Code);
        Assert.Equal("Vendor type not found", result.Error?.Description);
    }

    [Fact]
    public async Task GetVendorTypeByIdAsync_WhenFound_ReturnsMappedDto()
    {
        var vendorType = new VendorType { Id = Guid.NewGuid(), Name = "Catering" };
        _repositoryMock
            .Setup(x => x.GetVendorTypeByIdAsync(vendorType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendorType);

        var result = await _sut.GetVendorTypeByIdAsync(vendorType.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(vendorType.Id, result.Value.Id);
        Assert.Equal(vendorType.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetVendorTypesAsync_ReturnsMappedList()
    {
        var vendorTypes = new List<VendorType>
        {
            new() { Id = Guid.NewGuid(), Name = "Venue" },
            new() { Id = Guid.NewGuid(), Name = "Decor" }
        };
        _repositoryMock
            .Setup(x => x.GetVendorTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendorTypes);

        var result = await _sut.GetVendorTypesAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(vendorTypes.Select(v => v.Name), result.Value.Select(v => v.Name));
    }

    [Fact]
    public async Task UpdateVendorTypeAsync_WhenMissing_ReturnsNotFoundAndDoesNotUpdate()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetVendorTypeByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorType?)null);

        var result = await _sut.UpdateVendorTypeAsync(
            id,
            new CreateOrUpdateVendorTypeRequest { Name = "Updated" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Code);
        _repositoryMock.Verify(
            x => x.UpdateVendorTypeAsync(It.IsAny<VendorType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateVendorTypeAsync_WhenFound_UpdatesEntityAndReturnsDto()
    {
        var vendorType = new VendorType { Id = Guid.NewGuid(), Name = "Old" };
        _repositoryMock
            .Setup(x => x.GetVendorTypeByIdAsync(vendorType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendorType);

        var result = await _sut.UpdateVendorTypeAsync(
            vendorType.Id,
            new CreateOrUpdateVendorTypeRequest { Name = "Updated" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", vendorType.Name);
        Assert.Equal("Updated", result.Value.Name);
        _repositoryMock.Verify(
            x => x.UpdateVendorTypeAsync(vendorType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteVendorTypeAsync_DeletesAndReturnsTrue()
    {
        var id = Guid.NewGuid();

        var result = await _sut.DeleteVendorTypeAsync(id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        _repositoryMock.Verify(x => x.DeleteVendorTypeAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
