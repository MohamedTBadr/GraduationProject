using Domain.Contracts.Caching.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts.Caching
{
        public class CacheRepository(IConnectionMultiplexer multiplexer) : ICacheRepository
        {
            private readonly IDatabase _database = multiplexer.GetDatabase();
            public async Task<string> GetCachedValueAsync(string key)
            {
               return (await _database.StringGetAsync(key))!;
            }

            public async Task SetCacheValueAsync(string key, string value, TimeSpan expiration)
            {
               await _database.StringSetAsync(key, value, expiration);
            }
    }
}
