using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments.Index.Data
{
    public class IndexDocument : IAsyncDocument
    {
        public string Id { get; set; }

        public AsyncProperty<int> TotalQueries => new AsyncProperty<int>(this);
        public AsyncProperty<string> IndexHash => new AsyncProperty<string>(this);
        public AsyncProperty<DateTime> LastUpdated => new AsyncProperty<DateTime>(this);

        public async Task<bool> RequiresUpdate(string newHash) 
        {
            var hash = await IndexHash.GetAsync();
            if (string.Equals(newHash, hash))
                return false;

            return true;
        }

        public string IndexName()
        {
            return "index";
        }
    }
}
