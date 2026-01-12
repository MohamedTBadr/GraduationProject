using BLL.Services.Interfaces;
using DAL.Repositories.Caching;
using DAL.Repositories.Caching.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class HybridCacheService : ICacheService
    {
        private readonly ICacheRepository _memory;
        private readonly ICacheRepository _redis;

        public HybridCacheService(
            MemoryCacheRepository memory,
            CacheRepository redis)
        {
            _memory = memory;
            _redis = redis;
        }

        public async Task<string> GetCachedValueAsync(string key)
        {
            // L1
            var value = await _memory.GetCachedValueAsync(key);
            if (value != null)
                return value;

            // L2
            value = await _redis.GetCachedValueAsync(key);
            if (value != null)
            {
                await _memory.SetCacheValueAsync(key, value, TimeSpan.FromMinutes(1));
                return value;
            }

            return null;
        }

        public async Task SetCacheValueAsync(string key, string value, TimeSpan ttl)
        {
            await _redis.SetCacheValueAsync(key, value, ttl);
            await _memory.SetCacheValueAsync(key, value, TimeSpan.FromSeconds(30));
        }
    }

}
