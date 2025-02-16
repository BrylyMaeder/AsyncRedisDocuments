// See https://aka.ms/new-console-template for more information
using AsyncRedisDocuments;
using AsyncRedisDocuments.Index;
using AsyncRedisDocuments.QueryBuilder;
using Sample;


RedisSingleton.Initialize("redis-10220.c91.us-east-1-3.ec2.redns.redis-cloud.com", 10220, "ZrQZkNdkfJzpNwIWDdT2cZEgp5whsI7M");

for (int i = 0; i < 10; i++) 
{
    var car = new Car { Id = $"test{i}" };

    await car.Description.SetAsync("test description");
    await car.DisplayName.SetAsync("test");

    var car2 = new Car2 { Id = $"test{i}" };
    await car2.Description2.SetAsync("test2");
}

var results = await QueryBuilder.Query<Car>().SelectAsync(s => s.Description, s => s.DisplayName);

foreach (var result in results)
{
    Console.WriteLine(result.Properties);
}