using AsyncRedisDocuments;

namespace Sample
{
    public class User : IAsyncDocument
    {
        public string Id { get; set; }

        public AsyncLinkSet<Friendship> Friendships => new(this);
        public AsyncManagedLinkSet<Friendship> ManagedFriends => new(this);

        public AsyncLink<User> BestFriend => new(this);
        public ManagedLink<User> ManagedBestFriend => new(this);

        public StaticLink<Friendship> Friendship => new(this);

        public string IndexName()
        {
            return "users";
        }
    }
}
