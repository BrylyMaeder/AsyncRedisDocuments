﻿using AsyncRedisDocuments.Components;
using AsyncRedisDocuments.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AsyncRedisDocuments
{
    public struct IndexEntry
    {
        public string MemberName { get; set; }
        public IndexType IndexType { get; set; }
    }

    public struct DocumentAnalysisResult
    {
        public string IndexName { get; set; }
        public List<IndexEntry> IndexableEntries { get; set; }
    }

    public class DocumentAnalyzer
    {
        private static Dictionary<string, DocumentAnalysisResult> _cachedDocumentResults = new Dictionary<string, DocumentAnalysisResult>();

        public static DocumentAnalysisResult Analyze<TDocument>(TDocument document) where TDocument : IAsyncDocument
        {
            if (_cachedDocumentResults.TryGetValue(document.IndexName(), out var value))
            {
                return value;
            }

            var analysis = new DocumentAnalysisResult
            {
                IndexName = document.IndexName(),
                IndexableEntries = new List<IndexEntry>()
            };

            var visited = new HashSet<object>();

            Traverse(document, analysis.IndexableEntries, visited);

            _cachedDocumentResults[document.IndexName()] = analysis;

            return analysis;
        }

        private static void Traverse(object current, List<IndexEntry> entries, HashSet<object> visited)
        {
            if (current == null || visited.Contains(current)) return;

            visited.Add(current);

            var properties = current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var indexedAttribute = property.GetCustomAttribute<IndexedAttribute>();
                if (indexedAttribute != null)
                {
                    var indexType = indexedAttribute.IndexType;

                    if (indexType == IndexType.Auto)
                    {
                        Type propertyType = property.PropertyType;
                        if (propertyType.IsGenericType)
                        {
                            propertyType = propertyType.GetGenericArguments()[0]; // Get the type inside the generic
                        }

                        indexType = IndexTypeHelper.GetIndexType(propertyType);
                    }


                    var fullPath = property.Name;
                    var entry = new IndexEntry
                    {
                        MemberName = fullPath,
                        IndexType = indexType,
                    };

                    entries.Add(entry);
                }
            }
        }
    }
}