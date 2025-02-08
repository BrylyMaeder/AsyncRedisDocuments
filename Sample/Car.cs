using AsyncRedisDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Car : IAsyncDocument
    {
        public string Id { get; set; }

        public IndexedProperty<string> Description => new(this);
        public UniqueProperty<string> DisplayName => new(this);

        public AsyncProperty<ComplexModel> Model => new(this);
        
        public string IndexName()
        {
            return "cars";
        }
    }
}
