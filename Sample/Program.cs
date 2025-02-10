// See https://aka.ms/new-console-template for more information
using AsyncRedisDocuments;
using AsyncRedisDocuments.Index;
using Sample;


RedisSingleton.Initialize("host", port: 0000, "password");

var user = new User
{
    Id = "myUser",
};

var friendship = new Friendship {Id = Guid.NewGuid().ToString() };
await user.Friendships.AddOrUpdateAsync(friendship);
await user.Friendships.RemoveAsync(friendship);

