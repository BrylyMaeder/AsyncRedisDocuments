using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments
{
    public class UniqueAttribute : IndexedAttribute
    {
        public UniqueAttribute(IndexType indexType = IndexType.Tag) : base(indexType) { }
    }
}
