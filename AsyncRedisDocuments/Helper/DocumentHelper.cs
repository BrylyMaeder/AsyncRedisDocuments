
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments.Helper
{
    public class DocumentHelper
    {
        public static async Task DeleteDocumentAsync(IAsyncDocument document) 
        {
            if (document is IDeletionListener listener)
            {
                //inform a document listener that it's document is deleted.
                await listener.OnDeleted();
            }

            var properties = document.GetType().GetProperties();

            // Iterate all components and delete them
            foreach (var property in properties)
            {
                // Check if the property is readable and of a type that implements IDeletable
                if (property.CanRead)
                {
                    var value = property.GetValue(document);
                    if (value is IDeletable deletable)
                    {
                        // Call DeleteAsync on the IDeletable instance
                        await deletable.DeleteAsync();
                    }
                }
            }

            if (document is IAsyncDocument doc)
            {
                const int batchSize = 100; //Cleanup dead keys
                var cursor = 0L;

                try
                {
                    do
                    {
                        // Execute SCAN command to find keys with the specified pattern
                        var scanResult = await RedisSingleton.Database.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", $"{doc.GetKey()}*", "COUNT", batchSize);

                        // Parse the SCAN result
                        var resultArray = (RedisResult[])scanResult;
                        cursor = long.Parse(resultArray[0].ToString()); // Update cursor for next iteration
                        var keys = ((RedisResult[])resultArray[1]).Select(r => (RedisKey)r).ToArray(); // Collect keys

                        if (keys.Any())
                        {
                            // Batch delete keys asynchronously
                            await RedisSingleton.Database.KeyDeleteAsync(keys);
                        }
                    } while (cursor != 0); // Continue until cursor is 0

                }
                catch (Exception ex)
                {
                    // Log or handle errors here
                    Console.WriteLine($"Error during deletion: {ex.Message}");
                }
            }
            else throw new NotSupportedException("You cannot delete an entity that doesn't have a root IAsyncDocument.");          
        }
    }
}
