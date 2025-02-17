using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments
{
    public class RedisQuery
    {
        internal string IndexName { get; set; } = string.Empty;
        internal string Query { get; set; } = string.Empty;
        internal string LinqQuery { get; set; } = string.Empty;
    }

    public class RedisQuery<TDocument> : RedisQuery where TDocument : IAsyncDocument
    {
        
    }
}
