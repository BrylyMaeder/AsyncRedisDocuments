using AsyncRedisDocuments.Components;

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncDictionary<TKey, TValue> : BaseComponent
    {
        public AsyncDictionary(IAsyncDocument document = null, [CallerMemberName] string propertyName = null) : base(document, propertyName) { }

        // Check if a specific field key exists within the hash
        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            return await RedisSingleton.Database.HashExistsAsync(_fullKey, key.ConvertToRedisValue<TKey>());
        }

        // Set a value in the hash by field key
        public async Task<TValue> SetAsync(TKey key, TValue value)
        {
            var redisValue = value.ConvertToRedisValue<TValue>();
            await RedisSingleton.Database.HashSetAsync(_fullKey, new HashEntry[] { new HashEntry(key.ConvertToRedisValue<TKey>(), redisValue) });
            return value;
        }

        // Get a value from the hash by field key, or return a default value if the key does not exist
        public async Task<TValue> GetByKeyAsync(TKey key, TValue defaultValue = default)
        {
            var result = await RedisSingleton.Database.HashGetAsync(_fullKey, key.ConvertToRedisValue<TKey>());
            if (result.IsNullOrEmpty)
            {
                return defaultValue;
            }
            return result.ConvertFromRedisValue<TValue>();
        }


        // Remove a value from the hash by field key
        public async Task<bool> RemoveAsync(TKey key)
        {
            return await RedisSingleton.Database.HashDeleteAsync(_fullKey, key.ConvertToRedisValue<TKey>());
        }

        // Get the total count of items in the hash
        public async Task<long> CountAsync()
        {
            return await RedisSingleton.Database.HashLengthAsync(_fullKey);
        }

        // Get all values in the hash as a dictionary
        public async Task<Dictionary<TKey, TValue>> GetAsync()
        {
            var entries = await RedisSingleton.Database.HashGetAllAsync(_fullKey);
            return entries.ToDictionary(
                entry => entry.Name.ConvertFromRedisValue<TKey>(),
                entry => entry.Value.ConvertFromRedisValue<TValue>()
            );
        }
    }
}
