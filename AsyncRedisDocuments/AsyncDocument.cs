
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public abstract class AsyncDocument : IAsyncDocument, IDeletionListener
    {
        public string Id { get; set; }

        protected AsyncDocument(string id)
        {
            
        }

        public abstract string IndexName();

        public virtual async Task OnDeleted()
        {
            await Task.CompletedTask;
        }
    }
}
