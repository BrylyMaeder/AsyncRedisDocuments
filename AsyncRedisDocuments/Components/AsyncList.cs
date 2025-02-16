using AsyncRedisDocuments.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncList<TKey> : BaseComponent
    {
        public AsyncList(IAsyncDocument document = null, [CallerMemberName] string propertyName = null) : base(document, propertyName) 
        {

        }

        // Check if a specific value exists within the set
        public async Task<bool> ContainsAsync(TKey value)
        {
            return await RedisSingleton.Database.SetContainsAsync(_fullKey, value.ConvertToRedisValue<TKey>());
        }

        // Add a value to the set
        public async Task AddAsync(TKey value)
        {
            await RedisSingleton.Database.SetAddAsync(_fullKey, value.ConvertToRedisValue<TKey>());
        }

        // Add multiple values to the set
        public async Task AddRangeAsync(IEnumerable<TKey> values)
        {
            var redisValues = values.Select(v => v.ConvertToRedisValue<TKey>()).ToArray();
            if (redisValues.Any())
            {
                await RedisSingleton.Database.SetAddAsync(_fullKey, redisValues);
            }
        }

        // Remove a value from the set
        public async Task<bool> RemoveAsync(TKey value)
        {
            return await RedisSingleton.Database.SetRemoveAsync(_fullKey, value.ConvertToRedisValue<TKey>());
        }

        // Get the total count of items in the set
        public async Task<int> CountAsync()
        {
            return (int)await RedisSingleton.Database.SetLengthAsync(_fullKey);
        }

        // Get all values in the set
        public async Task<List<TKey>> GetAsync()
        {
            var values = await RedisSingleton.Database.SetMembersAsync(_fullKey);
            return values.Select(v => v.ConvertFromRedisValue<TKey>()).ToList();
        }

        // Clear all values in the set
        public async Task ClearAsync()
        {
            await RedisSingleton.Database.KeyDeleteAsync(_fullKey);
        }

        public async Task SetAsync(IEnumerable<TKey> values)
        {
            // Clear the existing set
            await ClearAsync();

            // Add new values to the set
            await AddRangeAsync(values);
        }
    }


}
