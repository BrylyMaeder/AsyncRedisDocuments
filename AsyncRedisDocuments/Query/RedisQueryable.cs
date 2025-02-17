using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace AsyncRedisDocuments
{
    public class RedisQueryable<T> : IQueryable<T>
    {
        private Expression _expression;
        private RedisQueryProvider _provider;

        public RedisQueryable(RedisQueryProvider provider)
        {
            _provider = provider;
            _expression = Expression.Constant(this);
        }

        public RedisQueryable(RedisQueryProvider provider, Expression expression)
        {
            _provider = provider;
            _expression = expression;
        }

        public Type ElementType => typeof(T);
        public Expression Expression => _expression;
        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator()
        {
            // In a real implementation, the query would be executed here.
            _provider.Execute<IEnumerable<T>>(_expression);
            // For demonstration, yield an empty sequence.
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
