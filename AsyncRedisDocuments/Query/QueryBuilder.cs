using System;
using System.Linq.Expressions;

namespace AsyncRedisDocuments
{
    public static class QueryBuilder
    {
        public static RedisQuery<TDocument> Query<TDocument>() where TDocument : IAsyncDocument
        {
            var doc = DocumentFactory.Create<TDocument>("");

            return new RedisQuery<TDocument>
            {
                IndexName = doc.IndexName(),
                Query = string.Empty,
                LinqQuery = string.Empty
            };
        }
        // Converts a LINQ expression into a RedisQuery object.
        public static RedisQuery<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> expression) where TDocument : IAsyncDocument
        {
            // Generate the query string.
            string queryString = BuildQuery(expression.Body);
            // Use the document type name as the index name (customize as needed).
            var doc = DocumentFactory.Create<TDocument>("");

            return new RedisQuery<TDocument>
            {
                IndexName = doc.IndexName(),
                Query = LinqToRedisConverter.Convert(queryString),
                LinqQuery = expression.ToString()
            };
        }

        // Generic expression tree to query string converter.
        internal static string BuildQuery(Expression expression)
        {
            var visitor = new RedisQueryVisitor();
            visitor.Visit(expression);
            return visitor.Query;
        }
    }
}
