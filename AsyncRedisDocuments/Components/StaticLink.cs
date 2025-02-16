using AsyncRedisDocuments.Components;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class StaticLink<TDocument> : BaseComponent, IDeletable where TDocument : IAsyncDocument
    {
        public StaticLink(IAsyncDocument document = null, [CallerMemberName] string propertyName = "") : base(document, propertyName)
        {
        }

        public TDocument Document => DocumentFactory.Create<TDocument>(_document.Id);

        public async Task DeleteAsync()
        {
            await Document.DeleteAsync();
        }
    }
}
