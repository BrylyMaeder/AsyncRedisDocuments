using AsyncRedisDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Person : IAsyncDocument
    {
        public string Id { get; set; } 

        public AsyncProperty<string> FridgeName => new(this);
        public AsyncLink<Person> Link => new(this);

        public string IndexName()
        {
            return "blazorsFridge";
        }
    }
}
