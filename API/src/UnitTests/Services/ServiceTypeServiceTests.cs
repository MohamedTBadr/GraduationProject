using Application.DTOs.ServiceTypesDTOs;
using Application.Services;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class ServiceTypeServiceTests
{
    private readonly Mock<IServiceTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ServiceTypeService _sut;

    public ServiceTypeServiceTests()
    {
        _sut = new ServiceTypeService(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task AddTypeAsync_MapsPersistsAndReturnsDto()
    {
        var request = new CreateServiceTypeRequest { Name = "Catering", VendorTypeId = Guid.NewGuid() };
        var entity = new ServiceType { Id = Guid.NewGuid(), Name = request.Name, VendorTypeId = request.VendorTypeId };
        var dto = new ServiceTypeDTO { Id = entity.Id, Name = entity.Name, VendorTypeId = entity.VendorTypeId };
        _mapperMock.Setup(x => x.Map<ServiceType>(request)).Returns(entity);
        _mapperMock.Setup(x => x.Map<ServiceTypeDTO>(entity)).Returns(dto);

        var result = await _sut.AddTypeAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(dto, result.Value);
        _repositoryMock.Verify(x => x.AddTypeAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTypeAsync_WhenMissing_ReturnsNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetServiceTypeByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceType)null!);

        var result = await _sut.DeleteTypeAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Code);
        _repositoryMock.Verify(x => x.DeleteTypeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTypeAsync_WhenFound_DeletesAndReturnsDeletedDto()
    {
        var type = new ServiceType { Id = Guid.NewGuid(), Name = "Decor" };
        var dto = new ServiceTypeDTO { Id = type.Id, Name = type.Name };
        _repositoryMock.Setup(x => x.GetServiceTypeByIdAsync(type.Id, It.IsAny<CancellationToken>())).ReturnsAsync(type);
        _mapperMock.Setup(x => x.Map<ServiceTypeDTO>(type)).Returns(dto);

        var result = await _sut.DeleteTypeAsync(type.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(type.Id, result.Value.Id);
        _repositoryMock.Verify(x => x.DeleteTypeAsync(type.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllServiceTypesAsync_ReturnsMappedList()
    {
        var entities = new List<ServiceType> { new() { Id = Guid.NewGuid(), Name = "Decor" } };
        var dtos = new List<ServiceTypeDTO> { new() { Id = entities[0].Id, Name = "Decor" } };
        _repositoryMock.Setup(x => x.GetAllServiceTypesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<List<ServiceTypeDTO>>(entities)).Returns(dtos);

        var result = await _sut.GetAllServiceTypesAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(dtos, result.Value);
    }

    [Fact]
    public async Task GetServiceTypeByIdAsync_WhenMissing_ReturnsNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetServiceTypeByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceType)null!);

        var result = await _sut.GetServiceTypeByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Service type not found", result.Error?.Description);
    }

    [Fact]
    public async Task UpdateTypeAsync_WhenFound_MapsOntoExistingAndUpdates()
    {
        var id = Guid.NewGuid();
        var existing = new ServiceType { Id = id, Name = "Old" };
        var request = new UpdateServiceTypeRequest { Name = "New", VendorTypeId = Guid.NewGuid() };
        var dto = new ServiceTypeDTO { Id = id, Name = "New", VendorTypeId = request.VendorTypeId };
        _repositoryMock.Setup(x => x.GetServiceTypeByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _mapperMock.Setup(x => x.Map(request, existing)).Callback(() =>
        {
            existing.Name = request.Name;
            existing.VendorTypeId = request.VendorTypeId;
        });
        _mapperMock.Setup(x => x.Map<ServiceTypeDTO>(existing)).Returns(dto);

        var result = await _sut.UpdateTypeAsync(id, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", existing.Name);
        _repositoryMock.Verify(x => x.UpdateTypeAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }
}
