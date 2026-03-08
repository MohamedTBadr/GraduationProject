using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts.Caching.Interfaces
{
    public interface ICacheRepository
    {
        Task<string> GetCachedValueAsync(string key);
        Task SetCacheValueAsync(string key, string value, TimeSpan expiration );
    }
}
