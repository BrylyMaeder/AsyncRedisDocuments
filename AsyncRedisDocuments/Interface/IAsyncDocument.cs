using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    /// <summary>
    /// Represents an asynchronous document with an ID, collection name, and delete functionality.
    /// Implementers must ensure to delete linked documents (if required) when calling DeleteAsync.
    /// </summary>
    public interface IAsyncDocument
    {
        string Id { get; set; }
        string IndexName();
    }
}
