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

        public IndexedProperty(IAsyncDocument document, TValue defaultValue = default, Func<TValue, Task<TValue>> getProcessingTask = null, Func<TValue, Task<TValue>> setProcessingTask = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) : base(document, defaultValue, getProcessingTask, setProcessingTask, propertyName)
        {
            _indexName = ParseCollectionFromKey(document.GetKey());

            _indexType = IndexTypeHelper.GetIndexType<TValue>();
        }

        private static string ParseCollectionFromKey(string documentKey)
        {
            var parts = documentKey.Split(':');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid documentKey format. Expected format is 'indexName:documentId'.", nameof(documentKey));
            }

            return parts[0];
        }
    }
}
