// See https://aka.ms/new-console-template for more information
using AsyncRedisDocuments;
using AsyncRedisDocuments.Index;
using AsyncRedisDocuments.QueryBuilder;
using Sample;


RedisSingleton.Initialize("host", port: 10220, "password");


for (int i = 0; i < 10; i++) 
{
    var car = new Car { Id = $"test{i}" };

    await car.Description.SetAsync("test description");
    await car.DisplayName.SetAsync("test");

    var car2 = new Car2 { Id = $"test{i}" };
    await car2.Description2.SetAsync("test2");
}

var query =  QueryBuilder.Query<Car>(s => s.DisplayName == "test");
var results = await query.ToListAsync();

var test = LinqToRedisConverter.Convert(query);

foreach (var result in results)
{
    Console.WriteLine(result.Id);
}