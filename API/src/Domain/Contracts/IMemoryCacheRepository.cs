namespace Domain.Contracts.Caching
{
    public interface IMemoryCacheRepository
    {
        Task<string?> GetCachedValueAsync(string key);
        Task SetCacheValueAsync(string key, string value, TimeSpan expiration);
    }
}