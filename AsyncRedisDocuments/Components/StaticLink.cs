using AsyncRedisDocuments.Components;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class StaticLink<TDocument> : BaseComponent, IDeletable where TDocument : IAsyncDocument
    {
        public StaticLink(IAsyncDocument parent, [CallerMemberName] string propertyName = "") : base(parent, propertyName) { }

        public TDocument Document => DocumentFactory.Create<TDocument>(_document.Id);

        public async Task DeleteAsync()
        {
            await Document.DeleteAsync();
        }
    }
}
