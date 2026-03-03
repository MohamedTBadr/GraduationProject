using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface ICacheService
    {
        Task<string> GetCachedValueAsync(string cacheKey);
        Task SetCacheValueAsync(string cacheKey, string value,TimeSpan TTL);
    }
}
