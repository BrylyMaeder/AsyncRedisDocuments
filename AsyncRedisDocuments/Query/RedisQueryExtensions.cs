using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AsyncRedisDocuments;
using System.Linq.Expressions;
using AsyncRedisDocuments.Models;

namespace AsyncRedisDocuments
{
    public static class RedisQueryExtensions
    {
        public static RedisQuery<TDocument> Query<TDocument>(this RedisQuery<TDocument> query, Expression<Func<TDocument, bool>> expression) where TDocument : IAsyncDocument
        {
            var q = QueryBuilder.Query<TDocument>(expression);

            query.Query += $" {q.Query}";

            return query;
        }

        public static async Task<(List<TDocument> Documents, int TotalCount, int TotalPages)> ToPagedListAsync<TDocument>(this RedisQuery<TDocument> query, int page = 1, int pageSize = 1000) where TDocument : IAsyncDocument
        {
            var results = await RedisSearchFunctions.Execute(query, page, pageSize);

            var documents = results.DocumentIds.Select(s => DocumentFactory.Create<TDocument>(s)).ToList();

            return (documents, results.TotalCount, results.TotalPages);
        }

        public static async Task<List<TDocument>> ToListAsync<TDocument>(this RedisQuery<TDocument> query, int page = 1, int pageSize = 1000) where TDocument : IAsyncDocument
        {
            var results = await RedisSearchFunctions.Execute(query, page, pageSize);

            return results.DocumentIds.Select(s => DocumentFactory.Create<TDocument>(s)).ToList();
        }

        public static async Task<bool> AnyAsync<TDocument>(this RedisQuery<TDocument> query) where TDocument : IAsyncDocument
        {
            if (string.IsNullOrEmpty(query.Query))
                return false;

            var results = await RedisSearchFunctions.Execute(query, 1, 1);

            return results.DocumentIds.Count > 0;
        }

        internal static async Task<List<string>> SearchAsync(this RedisQuery query, int page = 1, int pageSize = 1000)
        {
            var results = await RedisSearchFunctions.Execute(query, page, pageSize);
            return results.DocumentIds;
        }

        public static async Task<List<SelectResult<TDocument>>> SelectAsync<TDocument>(
            this RedisQuery<TDocument> query,
            params Expression<Func<TDocument, object>>[] propertyExpressions)
            where TDocument : IAsyncDocument
        {
            // Extract the fields from the expressions
            var selectedFields = propertyExpressions
                .SelectMany(expr => RedisSearchFunctions.GetSelectedFields(expr))
                .Distinct()
                .ToList();

            var results = await RedisSearchFunctions.SelectAsync<TDocument>(query, selectedFields, 1, 1000);

            return results.Results;
        }

        public static async Task<(List<SelectResult<TDocument>> Results, int TotalCount, int TotalPages)> PagedSelectAsync<TDocument>(
            this RedisQuery<TDocument> query, int pageNumber, int pageSize,
            params Expression<Func<TDocument, object>>[] propertyExpressions)
            where TDocument : IAsyncDocument
        {
            // Extract the fields from the expressions
            var selectedFields = propertyExpressions
                .SelectMany(expr => RedisSearchFunctions.GetSelectedFields(expr))
                .Distinct()
                .ToList();

            return await RedisSearchFunctions.SelectAsync<TDocument>(query, selectedFields, pageNumber, pageSize);
        }
    }

    public class GlobalSettings 
    {
        public AsyncProperty<string> Setting1 => new AsyncProperty<string>();
        public AsyncProperty<int> Setting2 => new AsyncProperty<int>();
    }
}
