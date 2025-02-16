
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public static class DocumentFactory
    {
        public static TDocument Create<TDocument>(string id) where TDocument : IAsyncDocument
        {
            var instance = CreateEmpty(typeof(TDocument));

            // Redis queries return the ID with the index path included; let's strip that out:
            var index = instance.IndexName();
            id = id.Replace($"{index}:", "");

            instance.Id = id;

            return (TDocument)instance;
        }

        internal static IAsyncDocument CreateEmpty(Type asyncDocumentType)
        {
            if (asyncDocumentType == null)
                throw new ArgumentNullException(nameof(asyncDocumentType));

            // Check if the type implements IAsyncDocument
            if (!typeof(IAsyncDocument).IsAssignableFrom(asyncDocumentType))
                throw new InvalidOperationException($"{asyncDocumentType.FullName} does not implement IAsyncDocument");

            // Retrieve all constructors, ordered by the number of parameters in ascending order
            var constructors = asyncDocumentType.GetConstructors()
                .OrderBy(c => c.GetParameters().Length)
                .ToArray();

            // Find the constructor with the fewest parameters (starting from the smallest)
            var constructor = constructors.FirstOrDefault();

            if (constructor == null)
                throw new InvalidOperationException($"No public constructors found for {asyncDocumentType.FullName}");

            // If the constructor has parameters, create default values
            var parameters = constructor.GetParameters()
                .Select(p => GetDefaultValue(p.ParameterType))
                .ToArray();

            // Create an instance using the constructor and parameters
            var instance = (IAsyncDocument)constructor.Invoke(parameters);
            return instance;
        }

        private static object GetDefaultValue(Type type)
        {
            // Return null for reference types and default value for value types
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }


}
