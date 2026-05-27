using Application.DTOs.ServiceDTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared;
using Shared.Exceptions;
using Xunit;

namespace Application.UnitTests.Services;

public class ServiceServiceTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IVendorRepository> _vendorRepositoryMock = new();
    private readonly Mock<ISearchService> _searchServiceMock = new();
    private readonly ServiceService _sut;

    public ServiceServiceTests()
    {
        _sut = new ServiceService(
            _serviceRepositoryMock.Object,
            _fileServiceMock.Object,
            _mapperMock.Object,
            _vendorRepositoryMock.Object,
            _searchServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_LoadsSearchIdsAndReturnsMappedPage()
    {
        var serviceId = Guid.NewGuid();
        var services = new List<Service> { Service(serviceId) };
        var dtos = new List<ServiceDTO> { Dto(serviceId) };
        _searchServiceMock.Setup(x => x.SearchServicesAsync("decor", null, null, null)).ReturnsAsync([serviceId]);
        _serviceRepositoryMock.Setup(x => x.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.Single() == serviceId), It.IsAny<CancellationToken>())).ReturnsAsync(services);
        _mapperMock.Setup(x => x.Map<IEnumerable<ServiceDTO>>(services)).Returns(dtos);

        var result = await _sut.GetAllAsync(new PaginatedRequest { SearchTerm = "decor", PageIndex = 1, PageSize = 10 }, false, false, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_WithoutSearch_UsesRepositoryPagingAndVisibility()
    {
        var services = new List<Service> { Service() };
        var dtos = new List<ServiceDTO> { Dto(services[0].Id) };
        _serviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<PaginatedRequest>(), It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResponse<Service>(services, 20, 2, 5));
        _mapperMock.Setup(x => x.Map<IEnumerable<ServiceDTO>>(services)).Returns(dtos);

        var result = await _sut.GetAllAsync(new PaginatedRequest { PageIndex = 2, PageSize = 5 }, false, false, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value.TotalCount);
        Assert.Equal(2, result.Value.PageNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNotFound()
    {
        _serviceRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Service)null!);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Code);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenMissing_ThrowsNotFoundException()
    {
        _serviceRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Service)null!);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleStatusAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task ToggleStatusAsync_WhenFound_TogglesHiddenStatus(bool currentHidden, bool expectedHidden)
    {
        var service = Service();
        service.IsHidden = currentHidden;
        _serviceRepositoryMock.Setup(x => x.GetByIdAsync(service.Id, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        await _sut.ToggleStatusAsync(service.Id, CancellationToken.None);

        _serviceRepositoryMock.Verify(x => x.UpdateStatusAsync(service.Id, expectedHidden, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenVendorMissing_ReturnsNotFoundResult()
    {
        var request = CreateRequest();
        _vendorRepositoryMock.Setup(x => x.GetVendorByIdAsync(request.VendorId!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((Vendor?)null);

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.Error!.Code);
    }

    [Fact]
    public async Task CreateAsync_WithImages_UploadsImagesPersistsAndIndexes()
    {
        var request = CreateRequest();
        request.ServiceImages = [Mock.Of<IFormFile>(), Mock.Of<IFormFile>()];
        var vendorId = request.VendorId!.Value;
        var mapped = Service();
        var created = Service(mapped.Id);
        _vendorRepositoryMock.Setup(x => x.GetVendorByIdAsync(vendorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Vendor { UserId = vendorId });
        _mapperMock.Setup(x => x.Map<Service>(request)).Returns(mapped);
        _fileServiceMock.SetupSequence(x => x.Upload("Services", It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("img-1")
            .ReturnsAsync("img-2");
        _serviceRepositoryMock.Setup(x => x.CreateAsync(mapped, It.IsAny<CancellationToken>())).ReturnsAsync(created);
        _mapperMock.Setup(x => x.Map<ServiceDTO>(created)).Returns(Dto(created.Id));

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, mapped.ServiceImages.Count);
        _searchServiceMock.Verify(x => x.IndexServiceAsync(created), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsNotFound()
    {
        _serviceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.UpdateAsync(new UpdateServiceDTO { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Service not found", result.Error?.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithImages_ReplacesOldImagesAndIndexes()
    {
        var dto = new UpdateServiceDTO { Id = Guid.NewGuid(), Name = "New", Images = [Mock.Of<IFormFile>()] };
        var mapped = Service(dto.Id);
        var updated = Service(dto.Id);
        _serviceRepositoryMock.Setup(x => x.ExistsAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<Service>(dto)).Returns(mapped);
        _serviceRepositoryMock.Setup(x => x.GetServiceImagesAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync([new ServiceImage { ImagePath = "old" }]);
        _fileServiceMock.Setup(x => x.Upload("services", It.IsAny<IFormFile>(), It.IsAny<CancellationToken>())).ReturnsAsync("new");
        _serviceRepositoryMock.Setup(x => x.UpdateAsync(mapped, It.IsAny<CancellationToken>())).ReturnsAsync(updated);
        _mapperMock.Setup(x => x.Map<ServiceDTO>(updated)).Returns(Dto(updated.Id));

        var result = await _sut.UpdateAsync(dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _fileServiceMock.Verify(x => x.DeleteAsync(It.Is<List<string>>(keys => keys.Single() == "old"), It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(x => x.DeleteServiceImagesAsync(dto.Id, It.IsAny<CancellationToken>()), Times.Once);
        _searchServiceMock.Verify(x => x.IndexServiceAsync(updated), Times.Once);
    }

    [Fact]
    public async Task AddRatingAsync_WhenUserMissing_ThrowsBadRequestException()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.AddRatingAsync(new ServiceRatingRequest { ServiceId = Guid.NewGuid(), UserId = null }, CancellationToken.None));
    }

    [Fact]
    public async Task AddRatingAsync_WhenUserDidNotPurchase_ThrowsBadRequestException()
    {
        var request = new ServiceRatingRequest { ServiceId = Guid.NewGuid(), UserId = Guid.NewGuid(), Rating = 4, Review = "Good" };
        _serviceRepositoryMock.Setup(x => x.HasUserPurchasedAsync(request.UserId.Value, request.ServiceId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.AddRatingAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task AddRatingAsync_WhenPurchased_AddsRating()
    {
        var request = new ServiceRatingRequest { ServiceId = Guid.NewGuid(), UserId = Guid.NewGuid(), Rating = 4, Review = "Good" };
        _serviceRepositoryMock.Setup(x => x.HasUserPurchasedAsync(request.UserId.Value, request.ServiceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _sut.AddRatingAsync(request, CancellationToken.None);

        _serviceRepositoryMock.Verify(x => x.AddRatingAsync(It.Is<ServiceRating>(r => r.UserId == request.UserId && r.ServiceId == request.ServiceId && r.Rating == 4), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_DeletesFilesAndSearchIndex()
    {
        var id = Guid.NewGuid();
        _serviceRepositoryMock.Setup(x => x.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _serviceRepositoryMock.Verify(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _fileServiceMock.Verify(x => x.DeleteAsync(It.Is<List<string>>(keys => keys.Single() == id.ToString()), It.IsAny<CancellationToken>()), Times.Once);
        _searchServiceMock.Verify(x => x.RemoveServiceAsync(id), Times.Once);
    }

    [Fact]
    public async Task AIFilterAsync_WhenSearchReturnsIds_LoadsByIds()
    {
        var id = Guid.NewGuid();
        var services = new List<Service> { Service(id) };
        _searchServiceMock.Setup(x => x.SearchServicesAsync("Wedding", null, null, 1000)).ReturnsAsync([id]);
        _serviceRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(services);
        _mapperMock.Setup(x => x.Map<List<ServiceDTO>>(services)).Returns([Dto(id)]);

        var result = await _sut.AIFilterAsync(new AIRequest { EventTypeName = "Wedding", Budget = 1000 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        _serviceRepositoryMock.Verify(x => x.AIFilterAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AIFilterAsync_WhenSearchEmpty_FallsBackToRepositoryFilter()
    {
        var services = new List<Service> { Service() };
        _searchServiceMock.Setup(x => x.SearchServicesAsync(It.IsAny<string>(), null, null, It.IsAny<decimal?>())).ReturnsAsync([]);
        _serviceRepositoryMock.Setup(x => x.AIFilterAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(services);
        _mapperMock.Setup(x => x.Map<List<ServiceDTO>>(services)).Returns([Dto(services[0].Id)]);

        var result = await _sut.AIFilterAsync(new AIRequest { EventTypeName = "Wedding", Budget = 1000 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _serviceRepositoryMock.Verify(x => x.AIFilterAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RebuildSearchIndexAsync_IndexesEveryService()
    {
        var services = new List<Service> { Service(), Service() };
        _serviceRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<PaginatedRequest>(), It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new PaginatedResponse<Service>(services, 2, 1, int.MaxValue));

        await _sut.RebuildSearchIndexAsync();

        _searchServiceMock.Verify(x => x.IndexServiceAsync(It.IsAny<Service>()), Times.Exactly(2));
    }

    private static Service Service(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Decor",
        Description = "Decor service",
        Price = 100,
        ServiceImages = []
    };

    private static ServiceDTO Dto(Guid id) => new()
    {
        Id = id,
        Name = "Decor",
        Description = "Decor service",
        Price = 100,
        ServiceImages = []
    };

    private static CreateServiceRequest CreateRequest() => new()
    {
        Name = "Decor",
        Description = "Decor service",
        Price = 100,
        VendorId = Guid.NewGuid(),
        ServiceImages = [],
        EventTypeIds = []
    };
}
