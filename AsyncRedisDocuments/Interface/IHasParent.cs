using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments
{
    public interface IAsyncDescendant : IAsyncDocument
    {
        IAsyncDocument Parent { get; set; }
    }
}
