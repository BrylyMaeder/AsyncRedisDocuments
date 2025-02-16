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
        [Indexed]
        public AsyncProperty<string> Description => new(this);
        [Unique]
        public AsyncProperty<string> DisplayName => new(this);

        public string Id { get; set; }

        public string IndexName()
        {
            return "cars:car1";
        }
    }

    public class Car2 : IAsyncDocument 
    {
        [Indexed]
        public AsyncProperty<string> Description2 => new(this);

        public string Id { get; set; }

        public string IndexName()
        {
            return "cars:car2";
        }
    }
}
