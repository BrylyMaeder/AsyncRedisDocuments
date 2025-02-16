## Available on Nuget
```csharp
nuget install AsyncRedisDocuments
```

Complete documentation available on the [wiki](https://github.com/BrylyMaeder/AsyncRedisDocuments/wiki)

This system simplifies Redis management in .NET by providing easy-to-use components like **AsyncProperty** , **AsyncLink** , **AsyncDictionary** , and **RedisQuery** . Here's a quick guide to building documents and querying data.1. **Document Example** 

```csharp
public class Car : IAsyncDocument
{
    [Indexed]
    public AsyncProperty<string> Description { get; set; }
    
    [Unique]
    public AsyncProperty<string> DisplayName { get; set; }
    
    public string Id { get; set; }
    
    public string IndexName() => "cars";
}
```
2. **Querying Data** 
You can easily query Redis documents using LINQ or query expressions.

#### Example: Query with LINQ Expression 


```csharp
var query = QueryBuilder.Query<Car>(car => car.Description.Contains("fast"));
var results = await query.ToPagedListAsync<Car>(page: 1, pageSize: 10);
```

#### Example: Query for Documents 


```csharp
var query = QueryBuilder.Query<Car>();
var results = await query.ToListAsync<Car>(page: 1, pageSize: 10);
```

#### Example: Check if Document Exists 


```csharp
var query = QueryBuilder.Query<Car>(car => car.DisplayName == "Mustang");
bool exists = await query.AnyAsync<Car>();
```
3. **Creating and Using Links** 

```csharp
public class Owner : IAsyncDocument
{
    public string Id { get; set; }
    public AsyncLink<Car> CarLink { get; set; }
}
```

To link documents:


```csharp
var owner = new Owner { Id = "123" };
owner.CarLink.SetAsync("carId123");
```
4. **Global Settings** 
For global settings that don't require document-specific names:


```csharp
public class GlobalSettings : BaseComponent
{
    public AsyncProperty<string> ApplicationName { get; set; }
}
```

To use global settings:


```csharp
var settings = new GlobalSettings();
settings.ApplicationName.SetAsync("MyApp");
```
5. **Redis Querying** 
You can query the Redis database for specific fields and documents.


```csharp
var query = QueryBuilder.Query<Car>(car => car.Description == "fast");
var cars = await query.ToListAsync<Car>();
```

## Authors

- [@BrylyMaeder](https://www.github.com/BrylyMaeder)

