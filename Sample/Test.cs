using AsyncRedisDocuments;

namespace Sample
{
    public class Test : IAsyncDocument
    {
        public string Id { get; set; }

        public IndexedProperty<string> Category => new(this);

        public UniqueProperty<string> DisplayName => new(this);

        public string IndexName()
        {
            return "test";
        }
    }
}
