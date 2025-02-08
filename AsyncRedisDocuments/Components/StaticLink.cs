using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class StaticLink<TDocument> : IDeletable where TDocument : IAsyncDocument
    {
        protected readonly IAsyncDocument _parent;
        protected readonly string _linkName;

        public StaticLink(IAsyncDocument document, [CallerMemberName] string linkName = "")
        {
            _parent = document;
            _linkName = linkName;
        }

        public TDocument Document => AsyncDocumentFactory.Create<TDocument>(_parent.Id);

        public async Task DeleteAsync()
        {
            await Document.DeleteAsync();
        }
    }
}
