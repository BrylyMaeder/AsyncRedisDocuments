using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments.Models
{
    public struct SelectResult<TDocument> where TDocument : IAsyncDocument
    {
        public TDocument Document;
        public Dictionary<string, object> Properties { get; set; }
    }

}
