using Application.Services.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Caching
{
    public class MemoryCacheRepository(IMemoryCache cache) : ICacheRepository
    {
        public Task<string?> GetCachedValueAsync(string key)
        {
            cache.TryGetValue(key, out string? value);
            return Task.FromResult(value);
        }

        public Task SetCacheValueAsync(string key, string value, TimeSpan expiration)
        {
            cache.Set(key, value, expiration);
            return Task.CompletedTask;
        }
    }

}
