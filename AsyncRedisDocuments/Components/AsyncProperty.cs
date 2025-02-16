using AsyncRedisDocuments.Components;
using AsyncRedisDocuments.Helper;
using AsyncRedisDocuments.Index;
using AsyncRedisDocuments.QueryBuilder;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncProperty<TValue> : BaseComponent
    {
        protected readonly TValue _defaultValue;
        protected readonly Func<TValue, Task<TValue>> _getProcessingTask;
        protected readonly Func<TValue, Task<TValue>> _setProcessingTask;

        protected string _indexName;
        protected IndexType _indexType;
        protected bool _isIndexed;
        protected bool _isUnique;

        public IndexType IndexType => _indexType;

        public AsyncProperty(IAsyncDocument document = null, TValue defaultValue = default,
            Func<TValue, Task<TValue>> getProcessingTask = null,
            Func<TValue, Task<TValue>> setProcessingTask = null,
            [CallerMemberName] string propertyName = null
        ) : base(document, propertyName)
        {
            _defaultValue = defaultValue;
            _getProcessingTask = getProcessingTask;
            _setProcessingTask = setProcessingTask;

            // Check if the property has an IndexedAttribute
            var propertyInfo = GetType().GetProperty(propertyName);
            var indexedAttribute = propertyInfo?.GetCustomAttribute<IndexedAttribute>();

            if (indexedAttribute != null)
            {
                _isIndexed = true;
                _indexType = indexedAttribute.IndexType;
                if (_indexType == IndexType.Auto)
                {
                    _indexType = IndexTypeHelper.GetIndexType<TValue>();
                }

                _isUnique = indexedAttribute is UniqueAttribute;

                if (_document == null)
                    throw new NotSupportedException("Document is required for Indexed or Unique properties.");

                _indexName = _document.IndexName();
            }
        }

        public virtual async Task<TValue> GetAsync()
        {
            var value = await RedisSingleton.Database.HashGetAsync(_documentKey, _propertyName);
            TValue result = value.IsNull ? _defaultValue : value.ConvertFromRedisValue<TValue>();

            if (_getProcessingTask != null)
                result = await _getProcessingTask(result);

            return result;
        }

        public virtual async Task<bool> SetAsync(TValue value)
        {
            if (_setProcessingTask != null)
                value = await _setProcessingTask(value);

            if (_isUnique)
            {
                var query = new RedisQuery();
                query.IndexName = _indexName;
                query.Query = $"{_propertyName}";

                var redisValue = value.ConvertToRedisValue<TValue>();

                // Index Type will always be TAG for "UniqueAttribute"
                query.Query = QueryHelper.Tag(_propertyName, redisValue.ToString());

                var results = await query.SearchAsync();
                if (results.Count > 0)
                    return false;
            }

            await RedisSingleton.Database.HashSetAsync(_documentKey, _propertyName, value.ConvertToRedisValue<TValue>());
            return true;
        }

        public virtual async Task ClearAsync()
        {
            await RedisSingleton.Database.HashDeleteAsync(_documentKey, _propertyName);
        }

        public static implicit operator TValue(AsyncProperty<TValue> obj) => default;
    }

}
