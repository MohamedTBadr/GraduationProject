using Application.Services.Helpers;
using Domain.Contracts.Caching.Interfaces;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class HybridCacheServiceTests
{
    private readonly Mock<ICacheRepository> _memoryMock = new();
    private readonly Mock<ICacheRepository> _redisMock = new();
    private readonly HybridCacheService _sut;

    public HybridCacheServiceTests()
    {
        _sut = new HybridCacheService(_memoryMock.Object, _redisMock.Object);
    }

    [Fact]
    public async Task GetCachedValueAsync_WhenMemoryHasValue_ReturnsMemoryValueWithoutRedis()
    {
        _memoryMock.Setup(x => x.GetCachedValueAsync("key")).ReturnsAsync("memory-value");

        var result = await _sut.GetCachedValueAsync("key");

        Assert.Equal("memory-value", result);
        _redisMock.Verify(x => x.GetCachedValueAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCachedValueAsync_WhenRedisHasValue_PopulatesMemory()
    {
        _memoryMock.Setup(x => x.GetCachedValueAsync("key")).Returns(Task.FromResult<string>(null!));
        _redisMock.Setup(x => x.GetCachedValueAsync("key")).ReturnsAsync("redis-value");

        var result = await _sut.GetCachedValueAsync("key");

        Assert.Equal("redis-value", result);
        _memoryMock.Verify(x => x.SetCacheValueAsync("key", "redis-value", TimeSpan.FromMinutes(1)), Times.Once);
    }

    [Fact]
    public async Task GetCachedValueAsync_WhenBothCachesMiss_ReturnsNull()
    {
        _memoryMock.Setup(x => x.GetCachedValueAsync("key")).Returns(Task.FromResult<string>(null!));
        _redisMock.Setup(x => x.GetCachedValueAsync("key")).Returns(Task.FromResult<string>(null!));

        var result = await _sut.GetCachedValueAsync("key");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetCacheValueAsync_WritesRedisWithRequestedTtlAndMemoryWithShortTtl()
    {
        var ttl = TimeSpan.FromHours(1);

        await _sut.SetCacheValueAsync("key", "value", ttl);

        _redisMock.Verify(x => x.SetCacheValueAsync("key", "value", ttl), Times.Once);
        _memoryMock.Verify(x => x.SetCacheValueAsync("key", "value", TimeSpan.FromSeconds(30)), Times.Once);
    }
}
