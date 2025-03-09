using AsyncRedisDocuments.Components;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncLink<TDocument> : BaseComponent where TDocument : IAsyncDocument
    {
        protected readonly string _linkName;

        public AsyncLink(IAsyncDocument document = null, [CallerMemberName] string propertyName = "") : base(document, propertyName) 
        {
            _linkName = propertyName;
        }

        public virtual async Task SetAsync(string id)
        {
            await _linkedDocumentId.SetAsync(id);
        }

        public async Task SetAsync(TDocument document)
        {
            if (document == null)
            {
                await ClearAsync();
                return;
            }

            await SetAsync(document.Id);
        }

        public async Task<string> GetIdAsync()
        {
            var document = await GetAsync();
            return document?.Id;
        }

        public async Task<TDocument> GetAsync()
        {
            var documentId = await _linkedDocumentId.GetAsync();
            if (string.IsNullOrEmpty(documentId))
                return default;

            var document = DocumentFactory.Create<TDocument>(documentId);
            if (!await document.ExistsAsync())
                return default;

            return document;
        }

        public virtual async Task ClearAsync()
        {
            await _linkedDocumentId.ClearAsync();
        }

        protected AsyncProperty<string> _linkedDocumentId => new AsyncProperty<string>(_document, propertyName: _linkName);
    }


}
