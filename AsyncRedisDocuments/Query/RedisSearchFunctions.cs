using AsyncRedisDocuments.Index.Data;
using AsyncRedisDocuments.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public static class RedisSearchFunctions
    {
        internal static async Task<(List<SelectResult<TDocument>> Results, int TotalCount, int TotalPages)> SelectAsync<TDocument>(
    RedisQuery<TDocument> query,
    List<string> selectedFields,
    int pageNumber = 1,
    int pageSize = 1000)
    where TDocument : IAsyncDocument
        {
            try
            {
                int offset = (pageNumber - 1) * pageSize;

                // Use a default query if the input query is null or empty
                string searchQuery = string.IsNullOrWhiteSpace(query.Query) ? "*" : query.Query;

                var redisQuery = new List<object>
        {
            query.IndexName,
            searchQuery,
            "LIMIT", offset, pageSize,
            "RETURN", selectedFields.Count
        };

                redisQuery.AddRange(selectedFields);

                var result = await RedisSingleton.Database.ExecuteAsync("FT.SEARCH", redisQuery.ToArray());
                var resultsArray = (RedisResult[])result;

                if (resultsArray.Length == 0)
                {
                    return (new List<SelectResult<TDocument>>(), 0, 0);
                }

                int totalCount = (int)resultsArray[0]; // First element is the total count
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var parsedResults = ParseResults<TDocument>(resultsArray, selectedFields);

                return (parsedResults, totalCount, totalPages);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error at ExecuteSelect: {e.Message}");
                return (new List<SelectResult<TDocument>>(), 0, 0);
            }
        }




        internal static List<string> GetSelectedFields<TDocument>(Expression<Func<TDocument, object>> expression)
        {
            if (expression.Body is NewExpression newExpr)
            {
                return newExpr.Members.Select(m => m.Name).ToList();
            }
            else if (expression.Body is MemberExpression memberExpr)
            {
                return new List<string> { memberExpr.Member.Name };
            }
            else if (expression.Body is UnaryExpression unaryExpr)
            {
                return GetSelectedFields<TDocument>(Expression.Lambda<Func<TDocument, object>>(unaryExpr.Operand, expression.Parameters));
            }

            throw new ArgumentException("Unsupported select expression.");
        }



        internal static List<SelectResult<TDocument>> ParseResults<TDocument>(RedisResult[] resultsArray, List<string> fields) where TDocument : IAsyncDocument
        {
            var resultList = new List<SelectResult<TDocument>>();

            for (int i = 1; i < resultsArray.Length; i += 2) // Loop through results
            {
                var document = DocumentFactory.Create<TDocument>(resultsArray[i].ToString());

                var fieldValues = (RedisResult[])resultsArray[i + 1]; // Get properties

                var properties = new Dictionary<string, object>();

                for (int j = 0; j < fieldValues.Length; j += 2) // Process field name/value pairs
                {
                    properties[fields[j / 2]] = fieldValues[j + 1].ToString();
                }

                resultList.Add(new SelectResult<TDocument> { Document = document, Properties = properties });
            }

            return resultList;
        }

        internal static async Task<(List<string> DocumentIds, int TotalCount, int TotalPages)> Execute(RedisQuery query, int pageNumber = 1, int pageSize = 1000)
        {
            try
            {
                int skipCount = (pageNumber - 1) * pageSize;
                int takeCount = pageSize;

                // Use a default query if the input query is null or empty
                string searchQuery = string.IsNullOrWhiteSpace(query.Query) ? "*" : query.Query;

                var result = await RedisSingleton.Database.ExecuteAsync(
                    "FT.SEARCH",
                    query.IndexName,
                    searchQuery,
                    "NOCONTENT",
                    "LIMIT",
                    skipCount,
                    takeCount
                );

                var resultsArray = (RedisResult[])result;
                if (resultsArray.Length == 0)
                {
                    return (new List<string>(), 0, 0);
                }

                int totalCount = (int)resultsArray[0]; // First element is the total count
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var documentIds = new List<string>();
                for (int i = 1; i < resultsArray.Length; i++)
                {
                    documentIds.Add(resultsArray[i].ToString());
                }

                var doc = new IndexDocument { Id = query.IndexName };
                var totalQueries = await doc.TotalQueries.GetAsync();
                totalQueries++;
                await doc.TotalQueries.SetAsync(totalQueries);

                return (documentIds, totalCount, totalPages);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error at SearchAsync: {e.StackTrace}");
                return (new List<string>(), 0, 0);
            }
        }

    }
}
