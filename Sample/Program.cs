// See https://aka.ms/new-console-template for more information
using AsyncRedisDocuments;
using AsyncRedisDocuments.Index;
using AsyncRedisDocuments.QueryBuilder;
using Sample;


RedisSingleton.Initialize("host", port:10000, "password");

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