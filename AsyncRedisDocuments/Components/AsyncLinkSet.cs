﻿using AsyncRedisDocuments.Components;

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
    public class AsyncLinkSet<TDocument> : BaseComponent where TDocument : IAsyncDocument
    {
        public AsyncLinkSet(IAsyncDocument document = null, [CallerMemberName] string propertyName = "") : base(document, propertyName)
        {

        }

        public virtual async Task SetAsync(List<TDocument> documents)
        {
            await RedisSingleton.Database.KeyDeleteAsync(_fullKey); // Clear existing set

            if (documents != null && documents.Any())
            {
                var documentKeys = documents.Select(doc => (RedisValue)doc.Id).ToArray();
                await RedisSingleton.Database.SetAddAsync(_fullKey, documentKeys);
            }
        }

        public virtual async Task<List<TDocument>> GetAllAsync()
        {
            var documentIds = await RedisSingleton.Database.SetMembersAsync(_fullKey);
            return documentIds.Select(value => DocumentFactory.Create<TDocument>(value.ToString())).ToList();
        }

        public virtual async Task<Dictionary<string, TDocument>> GetAsDictionaryAsync()
        {
            var documentIds = await RedisSingleton.Database.SetMembersAsync(_fullKey);
            var dictionary = new Dictionary<string, TDocument>();

            foreach (var value in documentIds)
            {
                var key = value.ToString();
                var document = DocumentFactory.Create<TDocument>(key);
                dictionary[key] = document;
            }

            return dictionary;
        }

        public virtual async Task<bool> ContainsAsync(IAsyncDocument document) => await ContainsAsync(document.Id);

        public virtual async Task<bool> ContainsAsync(string id)
        {
            return await RedisSingleton.Database.SetContainsAsync(_fullKey, id);
        }

        public virtual async Task<bool> AddOrUpdateAsync(TDocument document)
        {
            if (document == null) return false;

            return await RedisSingleton.Database.SetAddAsync(_fullKey, document.Id);
        }

        public virtual async Task<TDocument> GetAsync(IAsyncDocument document) => await GetAsync(document.Id);

        public virtual async Task<TDocument> GetAsync(string id)
        {
            if (!await ContainsAsync(id))
                return default;

            return DocumentFactory.Create<TDocument>(id);
        }

        public virtual async Task<bool> RemoveAsync(IAsyncDocument document) => await RemoveAsync(document.Id);

        public virtual async Task<bool> RemoveAsync(string id)
        {
            return await RedisSingleton.Database.SetRemoveAsync(_fullKey, id);
        }

        public virtual async Task<int> CountAsync()
        {
            return (int)await RedisSingleton.Database.SetLengthAsync(_fullKey);
        }

        public virtual async Task ClearAsync()
        {
            await RedisSingleton.Database.KeyDeleteAsync(_fullKey);
        }
    }

}
