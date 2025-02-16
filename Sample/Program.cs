// See https://aka.ms/new-console-template for more information
using AsyncRedisDocuments;
using AsyncRedisDocuments.Index;
using AsyncRedisDocuments.QueryBuilder;
using Sample;


RedisSingleton.Initialize("redis-13464.c81.us-east-1-2.ec2.redns.redis-cloud.com", 13464, "4TdQe8UepIdXwrGBGSJwTl5s1nsvYpgN");

for (int i = 0; i < 10; i++) 
{
    var car = new Car { Id = $"test{i}" };

    await car.Description2.SetAsync("test description");
    await car.DisplayName.SetAsync("test");
}

var results = await QueryBuilder.Query<Car>().SelectAsync(s => s.Description2, s => s.DisplayName);

foreach (var result in results)
{
    Console.WriteLine(result.Properties);
}