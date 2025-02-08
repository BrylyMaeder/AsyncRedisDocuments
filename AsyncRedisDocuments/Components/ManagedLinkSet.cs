using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncManagedLinkSet<TDocument> : AsyncLinkSet<TDocument>, IDeletable where TDocument : IAsyncDocument
    {
        public AsyncManagedLinkSet(IAsyncDocument document, [CallerMemberName] string listName = "")
            : base(document, listName)
        {
        }

        public new async Task SetAsync(List<TDocument> documents)
        {
            var existingIds = await RedisSingleton.Database.SetMembersAsync(_setKey);
            foreach (var id in existingIds.Select(value => value.ToString()))
            {
                var doc = AsyncDocumentFactory.Create<TDocument>(id);
                await doc.DeleteAsync();
            }

            await base.SetAsync(documents);
        }

        public new async Task<bool> RemoveAsync(string id)
        {
            var removed = await base.RemoveAsync(id);

            if (removed)
            {
                var document = AsyncDocumentFactory.Create<TDocument>(id);
                await document.DeleteAsync();
            }

            return removed;
        }

        public new async Task ClearAsync()
        {
            var documentIds = await RedisSingleton.Database.SetMembersAsync(_setKey);
            foreach (var id in documentIds.Select(value => value.ToString()))
            {
                var document = AsyncDocumentFactory.Create<TDocument>(id);
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
