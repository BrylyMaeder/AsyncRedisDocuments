using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AsyncRedisDocuments
{
    public static class DocumentFactory
    {
        public static TDocument Create<TDocument>(string id) where TDocument : IAsyncDocument 
        {
            var instance = Activator.CreateInstance<TDocument>();
            instance.Id = id;

            return instance;
        }
    }
}
