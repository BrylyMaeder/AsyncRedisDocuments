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

namespace AsyncRedisDocuments.Index.Generation
{
    public static class IndexBuilder
    {
        private static readonly List<Type> _indexedDocuments = new List<Type>();

        public static void InitializeIndexes()
        {
            foreach (var asyncDocumentType in GetAllAsyncDocumentTypes())
            {
                if (!_indexedDocuments.Contains(asyncDocumentType))
                {
                    _indexedDocuments.Add(asyncDocumentType);

                    // Use Activator to create an instance of the actual type
                    var instance = DocumentFactory.CreateEmpty(asyncDocumentType);
                    EnsureIndexAsync(instance).Wait();
                }
            }
        }

        public static async Task EnsureIndexAsync(IAsyncDocument document)
        {
            var indexName = document.IndexName();
            var result = IndexDefinitionBuilder.Build(document);

            if (await new IndexDocument { Id = indexName }.RequiresUpdate(result.IndexHash))
                await UpdateIndexAsync(indexName, result.IndexDefinition, result.IndexHash);
        }

        public static bool HasIndexableProperties(Type asyncDocumentType) =>
            asyncDocumentType.GetProperties()
                .Any(prop => Attribute.IsDefined(prop, typeof(IndexedAttribute)));


        public static IEnumerable<Type> GetAllAsyncDocumentTypes() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IAsyncDocument).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                .GroupBy(type => type.FullName)
                .Select(group => group.First());

        private static async Task UpdateIndexAsync(string indexName, RediSearchIndexDefinition definition, string hash)
        {
            try { await RedisSingleton.Database.DropIndexAsync(indexName); }
            catch { /* Ignored */ }
            await CreateNewIndexAsync(indexName, definition, hash);
        }

        private static async Task<IndexDocument> CreateNewIndexAsync(string indexName, RediSearchIndexDefinition definition, string hash)
        {
            var indexDocument = new IndexDocument { Id = indexName };
            if (definition != null && !string.IsNullOrEmpty(hash))
            {
                await RedisSingleton.Database.CreateIndexAsync(indexName, definition);
                await indexDocument.IndexHash.SetAsync(hash);
            }
            await indexDocument.LastUpdated.SetAsync(DateTime.UtcNow);
            return indexDocument;
        }
    }


}
