using AsyncRedisDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Friendship : IAsyncDocument
    {
        public string Id { get;set; }

        public AsyncProperty<DateTime> CreatedDate => new(this);

        public AsyncLink<User> Initiator => new(this);
        public AsyncLink<User> Receiver => new(this);

        public string IndexName()
        {
            return "friends";
        }
    }
}
