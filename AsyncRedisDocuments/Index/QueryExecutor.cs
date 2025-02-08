using AsyncRedisDocuments.Index.Data;
using RediSearchClient;
using RediSearchClient.Query;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments.Index
{
    public static class QueryExecutor
    {
        public static Query<TDocument> Query<TDocument>() where TDocument : IAsyncDocument, new()
        {
            return Index.Query<TDocument>.Create();
        }

        public static Query Query()
        {
            return Index.Query.Create();
        }

        public static async Task<List<TDocument>> GetAllAsync<TDocument>() where TDocument : IAsyncDocument
        {
            var tempInstance = AsyncDocumentFactory.Create<TDocument>(null);
            var indexName = tempInstance.IndexName();

            var pattern = $"{indexName}:*";

            var results = new List<TDocument>();

            var cursor = 0L;
            do
            {
                // Use SCAN with the restrictive pattern
                var scanResult = await RedisSingleton.Database.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", 1000);

                var resultArray = (RedisResult[])scanResult;
                cursor = (long)resultArray[0]; // Update cursor for the next iteration
                var keys = (RedisKey[])resultArray[1]; // Extract the keys from the result

                // Iterate over the found keys
                foreach (var key in keys)
                {
                    var keyString = key.ToString();
                    // Skip keys that have more than one colon (indexName:id:somethingElse)
                    if (!keyString.Contains(":") || keyString.Count(c => c == ':') > 1) continue;

                    var document = AsyncDocumentFactory.Create<TDocument>(key);
                    results.Add(document);
                }

            } while (cursor != 0); // Continue scanning until the cursor is 0 (end of the scan)

            return results;
        }
    }

}
