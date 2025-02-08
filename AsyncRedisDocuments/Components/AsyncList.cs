using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncList<TKey>
    {
        private string _primaryKey;

        public AsyncList(IAsyncDocument document, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            _primaryKey = $"{document.GetKey()}:{propertyName}";
        }

        // Check if a specific value exists within the set
        public async Task<bool> ContainsAsync(TKey value)
        {
            return await RedisSingleton.Database.SetContainsAsync(_primaryKey, value.ConvertToRedisValue<TKey>());
        }

        // Add a value to the set
        public async Task AddAsync(TKey value)
        {
            await RedisSingleton.Database.SetAddAsync(_primaryKey, value.ConvertToRedisValue<TKey>());
        }

        // Remove a value from the set
        public async Task<bool> RemoveAsync(TKey value)
        {
            return await RedisSingleton.Database.SetRemoveAsync(_primaryKey, value.ConvertToRedisValue<TKey>());
        }

        // Get the total count of items in the set
        public async Task<long> CountAsync()
        {
            return await RedisSingleton.Database.SetLengthAsync(_primaryKey);
        }

        // Get all values in the set
        public async Task<List<TKey>> GetAsync()
        {
            var values = await RedisSingleton.Database.SetMembersAsync(_primaryKey);
            return values.Select(v => v.ConvertFromRedisValue<TKey>()).ToList();
        }
    }

}
