using System;
using System.Diagnostics;

namespace AsyncRedisDocuments
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
        public IndexType IndexType { get; }
        public bool UniqueValidation { get; }

        public IndexedAttribute(IndexType type = IndexType.Auto)
        {
            IndexType = type;
        }
    }
}
