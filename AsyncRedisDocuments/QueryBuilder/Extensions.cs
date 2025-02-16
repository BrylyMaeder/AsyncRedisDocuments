using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments.QueryBuilder
{
    public static class AsyncPropertyExtensions
    {
        public static bool StartsWith(this AsyncProperty<string> property, string value)
        {
            throw new NotImplementedException("This method is for LINQ expression translation only.");
        }

        public static bool EndsWith(this AsyncProperty<string> property, string value) 
        {
            throw new NotImplementedException("This method is for LINQ expression translation only.");
        }

        public static bool Contains(this AsyncProperty<string> property, string value) 
        {
            throw new NotImplementedException("This method is for LINQ expression translation only.");
        }
    }
}
