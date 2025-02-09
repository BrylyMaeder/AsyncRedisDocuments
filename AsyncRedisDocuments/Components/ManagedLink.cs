using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class ManagedLink<TDocument> : AsyncLink<TDocument>, IDeletable where TDocument : IAsyncDocument
    {
        public ManagedLink(IAsyncDocument document, [CallerMemberName] string linkName = "")
            : base(document, linkName)
        {
        }

        public override async Task SetAsync(string id)
        {
            // Delete the existing document if one is linked
            var currentId = await _linkedDocumentId.GetAsync();
            if (!string.IsNullOrEmpty(currentId))
            {
                var currentDocument = DocumentFactory.Create<TDocument>(currentId);
                await currentDocument.DeleteAsync();
            }

            await base.SetAsync(id);
        }

        public override async Task ClearAsync()
        {
            var currentId = await _linkedDocumentId.GetAsync();
            if (!string.IsNullOrEmpty(currentId))
            {
                var currentDocument = DocumentFactory.Create<TDocument>(currentId);
                await currentDocument.DeleteAsync();
            }

            await base.ClearAsync();
        }

        public async Task DeleteAsync()
        {
            await ClearAsync();
        }
    }
}
