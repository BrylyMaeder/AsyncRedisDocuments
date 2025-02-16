using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AsyncRedisDocuments
{
    public class StaticDocument : AsyncDocument
    {
        public StaticDocument([CallerMemberName] string descendantName = null) : base(descendantName) { }

        public override string IndexName()
        {
            return "static";
        }
    }
}
