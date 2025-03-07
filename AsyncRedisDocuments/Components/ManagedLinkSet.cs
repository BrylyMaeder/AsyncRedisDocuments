﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class ManagedLinkSet<TDocument> : AsyncLinkSet<TDocument>, IDeletable where TDocument : IAsyncDocument
    {
        public ManagedLinkSet(IAsyncDocument document = null, [CallerMemberName] string listName = "")
            : base(document, listName)
        {
        }

        public override async Task SetAsync(List<TDocument> documents)
        {
            var existingIds = await RedisSingleton.Database.SetMembersAsync(_fullKey);
            foreach (var id in existingIds.Select(value => value.ToString()))
            {
                var doc = DocumentFactory.Create<TDocument>(id);
                await doc.DeleteAsync();
            }

            await base.SetAsync(documents);
        }

        public override async Task<bool> RemoveAsync(string id)
        {
            var removed = await base.RemoveAsync(id);

            if (removed)
            {
                var document = DocumentFactory.Create<TDocument>(id);
                await document.DeleteAsync();
            }

            return removed;
        }

        public override async Task ClearAsync()
        {
            var documentIds = await RedisSingleton.Database.SetMembersAsync(_fullKey);
            foreach (var id in documentIds.Select(value => value.ToString()))
            {
                var document = DocumentFactory.Create<TDocument>(id);
                await document.DeleteAsync();
            }

            await base.ClearAsync();
        }

        public async Task DeleteAsync()
        {
            await ClearAsync();
        }
    }
}
