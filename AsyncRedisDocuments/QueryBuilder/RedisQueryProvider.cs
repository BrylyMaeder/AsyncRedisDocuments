using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace AsyncRedisDocuments.QueryBuilder
{
    public class RedisQueryProvider : IQueryProvider
    {
        // Single access point for LINQ queries.
        public static IQueryable<TDocument> Query<TDocument>() where TDocument : IAsyncDocument
        {
            return new RedisQueryable<TDocument>(new RedisQueryProvider());
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new RedisQueryable<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = expression.Type.GetGenericArguments()[0];
            var queryableType = typeof(RedisQueryable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryableType, new object[] { this, expression });
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // Build the Redis query string from the expression tree.
            string query = QueryBuilder.BuildQuery(expression);
            Console.WriteLine("Redis Query: " + query);
            // Here you would normally execute the query against Redis.
            // For demonstration, we return default data.
            return default(TResult);
        }

        public object Execute(Expression expression)
        {
            return Execute<object>(expression);
        }
    }
}
