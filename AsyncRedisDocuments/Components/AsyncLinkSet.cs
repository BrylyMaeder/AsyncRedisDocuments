using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncLinkSet<TDocument> where TDocument : IAsyncDocument
    {
        protected readonly string _setKey;

        public AsyncLinkSet(IAsyncDocument document, [CallerMemberName] string listName = "")
        {
            _setKey = $"{document.GetKey()}:{listName}";
        }

        public async Task SetAsync(List<TDocument> documents)
        {
            await RedisSingleton.Database.KeyDeleteAsync(_setKey); // Clear existing set

            if (documents != null && documents.Any())
            {
                var documentKeys = documents.Select(doc => (RedisValue)doc.Id).ToArray();
                await RedisSingleton.Database.SetAddAsync(_setKey, documentKeys);
            }
        }

        public async Task<List<TDocument>> GetAllAsync()
        {
            var documentIds = await RedisSingleton.Database.SetMembersAsync(_setKey);
            return documentIds.Select(value => AsyncDocumentFactory.Create<TDocument>(value.ToString())).ToList();
        }
        public async Task<bool> ContainsAsync(IAsyncDocument document) => await ContainsAsync(document.Id);

        public async Task<bool> ContainsAsync(string id)
        {
            return await RedisSingleton.Database.SetContainsAsync(_setKey, id);
        }

        public async Task<bool> AddAsync(TDocument document)
        {
            if (document == null) return false;

            return await RedisSingleton.Database.SetAddAsync(_setKey, document.Id);
        }

        public async Task<TDocument> GetAsync(IAsyncDocument document) => await GetAsync(document.Id);

        public async Task<TDocument> GetAsync(string id)
        {
            if (!await ContainsAsync(id))
                return default;

            return AsyncDocumentFactory.Create<TDocument>(id);
        }

        public async Task<bool> RemoveAsync(IAsyncDocument document) => await RemoveAsync(document.Id);

        public async Task<bool> RemoveAsync(string id)
        {
            return await RedisSingleton.Database.SetRemoveAsync(_setKey, id);
        }

        public async Task<int> CountAsync()
        {
            return (int)await RedisSingleton.Database.SetLengthAsync(_setKey);
        }

        public async Task ClearAsync()
        {
            await RedisSingleton.Database.KeyDeleteAsync(_setKey);
        }
    }

}
