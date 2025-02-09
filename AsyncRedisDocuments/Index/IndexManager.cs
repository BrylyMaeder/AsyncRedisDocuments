using AsyncRedisDocuments.Index.Data;
using RediSearchClient;
using RediSearchClient.Indexes;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsyncRedisDocuments.Index
{
    public static class IndexManager
    {
        /// <summary>
        /// Ensures indexes are created for all IAsyncDocument types with indexable properties.
        /// </summary>
        public static void InitializeIndexes()
        {
            var asyncDocumentTypes = GetAllAsyncDocumentTypes();

            foreach (var asyncDocumentType in asyncDocumentTypes)
            {
                var indexableProperties = GetPropertiesImplementingIIndexable(asyncDocumentType);

                if (indexableProperties.Any())
                {
                    var asyncDocumentInstance = (IAsyncDocument)Activator.CreateInstance(asyncDocumentType);
                    EnsureIndexAsync(asyncDocumentInstance).Wait();
                }
            }
        }

        /// <summary>
        /// Ensures the index for a given document type is up to date in Redis.
        /// </summary>
        public static async Task EnsureIndexAsync(IAsyncDocument document)
        {
            var indexName = document.IndexName();
            var result = IndexDefinitionBuilder.Build(document.GetType());

            var index = new IndexDocument { Id = indexName };

            if (await index.RequiresUpdate(result.IndexHash))
            {
                // Index exists but is outdated, update it
                await UpdateIndexAsync(indexName, result.IndexDefinition, result.IndexHash);
            }
        }


        /// <summary>
        /// Gets all properties in a type that implement IIndexable.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesImplementingIIndexable(Type asyncDocumentType)
        {
            return asyncDocumentType.GetProperties()
                .Where(prop => typeof(IIndexable).IsAssignableFrom(prop.PropertyType));
        }

        /// <summary>
        /// Retrieves all types in the current application domain that implement IAsyncDocument.
        /// </summary>
        public static IEnumerable<Type> GetAllAsyncDocumentTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IAsyncDocument).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);
        }

        private static async Task<IndexDocument> CreateNewIndexAsync(string indexName, RediSearchIndexDefinition definition, string hash)
        {
            // Create a new index on Redis
            var indexDocument = new IndexDocument { Id = indexName };

            if (definition != null && !string.IsNullOrEmpty(hash))
            {
                await RedisSingleton.Database.CreateIndexAsync(indexName, definition);
                await indexDocument.IndexHash.SetAsync(hash);
            }

            await indexDocument.LastUpdated.SetAsync(DateTime.UtcNow);

            return indexDocument;
        }

        private static async Task UpdateIndexAsync(string indexName, RediSearchIndexDefinition definition, string hash)
        {
            try
            {
                // Drop and recreate the index for changes
                await RedisSingleton.Database.DropIndexAsync(indexName);
            }
            catch 
            {
                // Probably wasn't an index available. doesn't matter, we just don't care bro
            }
            await CreateNewIndexAsync(indexName, definition, hash);
        }
    }

}
