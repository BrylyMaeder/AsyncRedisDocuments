using AsyncRedisDocuments.Index.Data;
using RediSearchClient;
using RediSearchClient.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments.Index
{
    public static class QueryExtensions
    {
        public static async Task<List<TDocument>> SearchAsync<TDocument>(this Query<TDocument> query) where TDocument : IAsyncDocument, new()
        {
            var tempInstance = DocumentFactory.Create<TDocument>(null);
            var indexName = tempInstance.IndexName();

            var results = await SearchAsync(indexName, query);

            return results.Select(id => DocumentFactory.Create<TDocument>(id)).ToList();
        }

        public static async Task<List<string>> SearchAsync(this Query query, string indexName)
        {
            return await SearchAsync(indexName, query);
        }

        public static async Task<bool> ContainsAsync<TDocument>(this Query<TDocument> query) where TDocument : IAsyncDocument, new()
        {
            return (await SearchAsync(query)).Count > 0;
        }

        public static async Task<bool> ContainsAsync(this Query query, string indexName)
        {
            return (await SearchAsync(indexName, query)).Count > 0;
        }

        private static async Task<List<string>> SearchAsync(string indexName, Query query)
        {
            try
            {
                var search = RediSearchQuery.On(indexName).UsingQuery(query.ToString()).NoContent().Build();
                var results = await RedisSingleton.Database.SearchAsync(search);

                var result = results?.RawResult
                                ?.Skip(1)
                                .Where(s => s != null) // Ensure no null elements
                                .Select(s => s.ToString())
                                .Where(str => str.Contains(':')) // Ensure ':' is present
                                .Select(str => str.Split(':').Last()) // Safe to split now
                                .ToList() ?? new List<string>(); // Default to empty list if any step fails

                var doc = new IndexDocument { Id = indexName };
                var totalQueries = await doc.TotalQueries.GetAsync();
                totalQueries++;
                await doc.TotalQueries.SetAsync(totalQueries);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error at SearchAsync: {e.StackTrace}");
                return new List<string>();
            }
        }
    }

}
