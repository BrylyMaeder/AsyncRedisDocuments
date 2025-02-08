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
        private static readonly List<IndexDocument> _indexData = new List<IndexDocument>();
        private static readonly object _lock = new object();

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

            if (_indexData.Any(s => s.Id.Equals(indexName)))
                return;

            await LoadIndexData();

            var result = IndexDefinitionBuilder.Build(document.GetType());

            var index = _indexData.FirstOrDefault(x => x.Id.Equals(indexName));

            if (index == null)
            {
                // Index doesn't exist, create it
                await CreateNewIndexAsync(indexName, result.IndexDefinition, result.IndexHash);
            }
            else if (await index.RequiresUpdate(result.IndexHash))
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

        private static async Task LoadIndexData()
        {
            // Fetch the current list of indexes from Redis
            var indexNames = await RedisSingleton.Database.ListIndexesAsync();

            lock (_lock)
            {
                _indexData.Clear();
                foreach (var indexName in indexNames)
                {
                    _indexData.Add(new IndexDocument { Id = indexName });
                }
            }
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

            lock (_lock)
            {
                _indexData.Add(indexDocument);
            }

            return indexDocument;
        }

        private static async Task UpdateIndexAsync(string indexName, RediSearchIndexDefinition definition, string hash)
        {
            // Drop and recreate the index for changes
            await RedisSingleton.Database.DropIndexAsync(indexName);
            await CreateNewIndexAsync(indexName, definition, hash);
        }
    }

}
