using AsyncRedisDocuments.Index;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class UniqueProperty<TValue> : IndexedProperty<TValue>
    {
        public UniqueProperty(IAsyncDocument document, TValue defaultValue = default, Func<TValue, Task<TValue>> getProcessingTask = null, Func<TValue, Task<TValue>> setProcessingTask = null, [CallerMemberName] string propertyName = null) : base(document, defaultValue, getProcessingTask, setProcessingTask, propertyName)
        {

        }

        public override async Task<bool> SetAsync(TValue value)
        {
            var redisValue = value.ConvertToRedisValue<TValue>();
            var query = QueryExecutor.Query();

            switch (_indexType)
            {
                case IndexType.Text:
                    query.TextExact(_propertyName, redisValue.ToString());
                    break;
                default:
                    query.Tag(_propertyName, redisValue.ToString());
                    break;
            }

            bool exists = await query.ContainsAsync(_indexName);
            if (exists)
                return false;

            return await base.SetAsync(value);
        }
    }
}
