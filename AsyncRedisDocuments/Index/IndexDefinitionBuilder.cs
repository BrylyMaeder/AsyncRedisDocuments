using RediSearchClient.Indexes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace AsyncRedisDocuments.Index
{
    public class IndexDefinitionBuilder
    {
        public static (RediSearchIndexDefinition IndexDefinition, string IndexHash) Build(Type documentType)
        {
            var indexName = GetIndexNameFromType(documentType);

            var schemaFields = new List<IRediSearchSchemaField>();
            var properties = GetProperties(documentType);
            var builder = new RediSearchSchemaFieldBuilder();

            // Use a StringBuilder to collect schema details for Base64 encoding
            var schemaDetailsBuilder = new StringBuilder();

            foreach (var pair in properties)
            {
                IRediSearchSchemaField field = null;

                var data = pair.Value;
                switch (data.IndexType)
                {
                    case IndexType.Tag:
                        field = builder.Tag(pair.Key);
                        break;
                    case IndexType.Text:
                        field = builder.Text(pair.Key);
                        break;
                    case IndexType.Numeric:
                        field = builder.Numeric(pair.Key);
                        break;
                }

                schemaFields.Add(field);

                // Append details deterministically for Base64 encoding
                schemaDetailsBuilder.Append(pair.Key).Append(":").Append(pair.Value.ClassType).Append(pair.Value.IndexType).Append(";");
            }

            if (schemaFields.Count == 0)
            {
                return (null, null);
            }

            // Convert schema details to a Base64 string
            var indexHash = schemaDetailsBuilder.ToString();

            var fields = schemaFields.ToArray();
            var definition = RediSearchIndex.OnHash()
                .ForKeysWithPrefix($"{indexName}:")
                .WithSchema(fields).Build();

            return (definition, indexHash);
        }

        public static string GetIndexNameFromType(Type documentType)
        {
            // Ensure the type implements IAsyncDocument
            if (!typeof(IAsyncDocument).IsAssignableFrom(documentType))
            {
                throw new InvalidOperationException($"{documentType.FullName} does not implement IAsyncDocument.");
            }

            // Create an instance of the type
            var doc = (IAsyncDocument)Activator.CreateInstance(documentType);

            return doc.IndexName();
        }

        // Helper method to compute a Base64 string
        private static string ComputeBase64(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }


        internal class PropertyData 
        {
            public IndexType IndexType { get; set; }
            public string ClassType { get; set; }
        }

        private static Dictionary<string, PropertyData> GetProperties(Type type)
        {
            var pairs = new Dictionary<string, PropertyData>();

            // Get all properties of the class
            PropertyInfo[] properties = type.GetProperties();

            // Loop through each property
            foreach (var property in properties)
            {
                // Check if the property's type is or derives from IndexedProperty<>
                if (IsSubclassOfRawGeneric(typeof(IndexedProperty<>), property.PropertyType))
                {
                    // Create an instance of TDocument and get the value of the property
                    var instance = Activator.CreateInstance(type);
                    var propertyInstance = property.GetValue(instance);

                    if (propertyInstance != null)
                    {
                        // Use nameof to dynamically reference the IndexType property
                        var indexTypeProperty = propertyInstance.GetType().GetProperty(nameof(IndexedProperty<object>.IndexType));
                        if (indexTypeProperty != null)
                        {
                            var indexTypeValue = (IndexType)indexTypeProperty.GetValue(propertyInstance);
                            pairs[property.Name] = new PropertyData 
                            {
                                IndexType = indexTypeValue,
                                ClassType = propertyInstance.GetType().Name
                            };
                        }
                    }
                }
            }

            return pairs;
        }

        // Helper method to check for raw generic type or its subclasses
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }


    }
}
