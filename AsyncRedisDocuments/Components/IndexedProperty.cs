using AsyncRedisDocuments.Index;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class IndexedProperty<TValue> : AsyncProperty<TValue>, IIndexable
    {
        protected string _indexName;

        protected IndexType _indexType;

        public IndexType IndexType => _indexType;

        public IndexedProperty(IAsyncDocument document, IndexType indexType = IndexType.Tag, TValue defaultValue = default, Func<TValue, Task<TValue>> getProcessingTask = null, Func<TValue, Task<TValue>> setProcessingTask = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) : base(document, defaultValue, getProcessingTask, setProcessingTask, propertyName)
        {
            _indexType = indexType;
            _indexName = document.IndexName();
        }
    }
}
