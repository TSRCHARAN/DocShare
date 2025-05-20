using DocumentsProject.Core.Domain.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task SetCacheAsync(string key, string value, TimeSpan? expiry = null)
        {
            await _database.StringSetAsync(key, value, expiry);
        }
        public async Task<string?> GetCacheAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }
        public async Task RemoveCacheAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
