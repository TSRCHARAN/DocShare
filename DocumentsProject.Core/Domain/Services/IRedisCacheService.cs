using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Domain.Services
{
    public interface IRedisCacheService
    {
        Task SetCacheAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetCacheAsync(string key);
        Task RemoveCacheAsync(string key);
    }

}
