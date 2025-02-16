## Available on Nuget
```csharp
nuget install AsyncRedisDocuments
```

This system provides a set of components and patterns for managing documents and related data structures in a Redis-backed document storage system. It allows for flexibility, scalability, and easy integration into various types of applications.

## Key Concepts & Components 
1. **AsyncProperty** `AsyncProperty` represents an asynchronous property on a document or object, simplifying the loading and saving of values to a Redis database. It supports complex types, time stamps, and other custom types, and automatically handles the conversion of values to and from Redis.**Example:** 

```csharp
public class Car : IAsyncDocument
{
    [Indexed]
    public AsyncProperty<string> Description => new(this);
    [Unique]
    public AsyncProperty<string> DisplayName => new(this);

    public string Id { get; set; }

    public string IndexName() => "cars";
}
```
2. **AsyncDictionary<TKey, TValue>** `AsyncDictionary` provides a way to store and manage a dictionary-like structure in Redis, using keys and values. Operations include adding, removing, checking for keys, and getting values asynchronously.**Example:** 

```csharp
var carDictionary = new AsyncDictionary<string, Car>();
await carDictionary.SetAsync("car1", carObject);
var car = await carDictionary.GetByKeyAsync("car1");
```
3. AsyncLink<TDocument>** `AsyncLink` represents a reference (or link) to another document. It stores the ID of the linked document and provides methods to get and set the linked document asynchronously.**Example:** 

```csharp
public class Car : IAsyncDocument
{
    public AsyncLink<Owner> OwnerLink { get; set; }

    public string Id { get; set; }
    public string IndexName() => "cars";
}

public class Owner : IAsyncDocument
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string IndexName() => "owners";
}
```
4. ManagedLink<TDocument>** `ManagedLink` is an extension of `AsyncLink` that adds additional functionality, such as automatically deleting the linked document when the link is changed or cleared.**Example:** 

```csharp
var carLink = new ManagedLink<Car>();
await carLink.SetAsync("car123");
```
5. AsyncLinkSet<TDocument>** `AsyncLinkSet` is used to manage a set of links (referencing multiple documents) in Redis. It allows operations like adding, removing, and querying links asynchronously.**Example:** 

```csharp
var linkSet = new AsyncLinkSet<Car>();
await linkSet.AddOrUpdateAsync(carObject);
```
6. ManagedLinkSet<TDocument>** `ManagedLinkSet` extends `AsyncLinkSet` by providing additional functionality to automatically delete linked documents when they are removed from the set.**Example:** 

```csharp
await linkSet.SetAsync(new List<Car> { carObject1, carObject2 });
```
7. StaticLink<TDocument>** `StaticLink` represents a reference to another document, where the linked document cannot be modified. This class is used for managing static, non-modifiable relationships.**Example:** 

```csharp
var staticLink = new StaticLink<Car>();
await staticLink.DeleteAsync();
```
8. AsyncList<TKey>** `AsyncList` provides a way to store and manage a set of values in Redis. It allows operations like adding, removing, and querying items asynchronously, as well as managing the set size.**Example:** 

```csharp
var asyncList = new AsyncList<string>();
await asyncList.AddAsync("value1");
await asyncList.RemoveAsync("value1");
```
9. **GlobalSettings** Global settings provide a simple way to manage global application settings without the need for document-specific configuration. `GlobalSettings` allows you to store key-value pairs globally, independent of any document.**Example:** 

```csharp
    public class GlobalSettings 
    {
        public AsyncProperty<string> Setting1 => new AsyncProperty<string>();
        public AsyncProperty<int> Setting2 => new AsyncProperty<int>();
    }
```

```csharp
var globalSettings = new GlobalSettings();
await globalSettings.Setting1.SetAsync("Some setting value");
await globalSettings.Setting2.SetAsync(42);

```
10. **Querying Documents** 
The system supports querying documents using LINQ and custom query strings. You can easily build queries to search for documents based on various criteria, and return paginated results.
**Example:** 

```csharp
var query = QueryBuilder.Query<Car>(x => x.Description.Contains("Sedan"));
var results = await query.ToPagedListAsync<Car>(page: 1, pageSize: 10);
```

## QueryBuilder 
 
- `Query<TDocument>`: Build a query for a specific document type.
 
- `Query<TDocument>(Expression<Func<TDocument, bool>> expression)`: Converts a LINQ expression into a Redis query.
**Example Query Usage:** 

```csharp
var query = QueryBuilder.Query<Car>(x => x.DisplayName.Contains("Tesla"));
var results = await query.ToPagedListAsync<Car>(1, 10);
```

### Redis Query Extensions 

The system includes various extension methods to simplify working with Redis queries:
 
- **`ToPagedListAsync<TDocument>()`** : Paginates results and returns documents, total count, and total pages.
 
- **`ToListAsync<TDocument>()`** : Returns a list of documents matching the query.
 
- **`AnyAsync<TDocument>()`** : Checks if any document matches the query.
 
- **`SelectAsync<TDocument>()`** : Selects specific fields from documents based on LINQ expressions.
 
- **`PagedSelectAsync<TDocument>()`** : Paginates results while selecting specific fields.

## Authors

- [@BrylyMaeder](https://www.github.com/BrylyMaeder)

