using System;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public class AsyncProperty<TValue>
    {
        protected readonly TValue _defaultValue;
        protected string _propertyName;
        protected string _documentKey;

        protected readonly IAsyncDocument _document;
        readonly Func<TValue, Task<TValue>> _getProcessingTask;
        readonly Func<TValue, Task<TValue>> _setProcessingTask;

        public AsyncProperty(IAsyncDocument document,
            TValue defaultValue = default,
            Func<TValue, Task<TValue>> getProcessingTask = null,
            Func<TValue, Task<TValue>> setProcessingTask = null,
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            _document = document;
            _documentKey = document.GetKey();
            _defaultValue = defaultValue;
            _propertyName = propertyName;
            _getProcessingTask = getProcessingTask;
            _setProcessingTask = setProcessingTask;
        }

        // Asynchronously retrieve the value without cancellation token
        public virtual async Task<TValue> GetAsync()
        {
            var value = await RedisSingleton.Database.HashGetAsync(_documentKey, _propertyName);
            TValue result = value.IsNull ? _defaultValue : value.ConvertFromRedisValue<TValue>();

            if (_getProcessingTask != null)
                result = await _getProcessingTask(result);

            return result;
        }

        // Asynchronously set the value without cancellation token
        public virtual async Task<bool> SetAsync(TValue value)
        {
            if (_setProcessingTask != null)
                value = await _setProcessingTask(value);

            await RedisSingleton.Database.HashSetAsync(_documentKey, _propertyName, value.ConvertToRedisValue<TValue>());
            return true;
        }

        public virtual async Task ClearAsync()
        {
            await RedisSingleton.Database.HashDeleteAsync(_documentKey, _propertyName);
        }
    }

}
