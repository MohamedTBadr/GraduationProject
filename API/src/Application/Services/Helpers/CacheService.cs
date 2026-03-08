using Application.Interfaces;
using Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Helpers
{
    internal class CacheService(ICacheRepository cacheRepository) : ICacheService
    {
        public async Task<string> GetCachedValueAsync(string cacheKey)
        {
         return  await  cacheRepository.GetCachedValueAsync(cacheKey);
        }

        public async Task SetCacheValueAsync(string cacheKey, string value, TimeSpan TTL)
        {

            await cacheRepository.SetCacheValueAsync(cacheKey, value, TTL);
        }
    }
}
