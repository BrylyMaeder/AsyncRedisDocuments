
# Async Redis Documents

Easily create data-driven models using asynchronous programming architecture, easily perform complex queries on your models and use them with ease.


## The Basics

IMPORTANT: Requires a Redis database with Search and Query enabled.


The RedisSingleton MUST be initialized.
```csharp
RedisSingleton.Initialize("host", port, "password");
```

Begin by implementing an IAsyncDocument

```csharp
    public class Car : IAsyncDocument
    {
        public string Id { get; set; }

        public string IndexName()
        {
            return "cars";
        }
    }
```

Next, add an AsyncProperty to your model like so

```csharp
        public AsyncProperty<string> Description => new(this);
```

Set the description like so:
```csharp
await car.Description.SetAsync("my description");
```

Load it back
```csharp
var description = await car.Description.GetAsync();
```

We can even store a complex model into the async property, which is stored as json in redis.

```csharp
    public class ComplexModel
    {
        public List<List<List<string>>> complexList = new List<List<List<string>>>();
        public string Name { get; set; }
    }
```
```csharp
public AsyncProperty<ComplexModel> Model => new(this);
```
## Geerated Indexes & Unique Validation

Indexes are automatically generated by the AsyncDocument system. An index is built where any IAsyncDocument contains a "IndexedProperty" or a "UniqueProperty". Each time you add or remove an IndexedProperty or UniqueProperty your redis index will internally be dropped and recreated to match your new model structure.

Index names are important, make sure you do not give a document an index name used elsewhere. My recommendation is to provide your model an index name similar to the class name.

```csharp
    public class User : IAsyncDocument
    {
        public string Id { get; set; }

        public UniqueProperty<string> DisplayName => new(this);

        public IndexedProperty<string> Biography => new(this);

        public string IndexName()
        {
            return "users";
        }
    }
```

The UniqueProperty adds a fast and convienent way to implement unique validation into any application. 
- Important: UniqueProperty automatically handles case insensitivity and does not currently have an option for case sensitivity.

```csharp
var user = new User
{
    Id = "myUser",
};

var succeeded = await user.DisplayName.SetAsync("TheLegend");

if (succeeded)
{
    Console.WriteLine("Your username was successfully changed.");
}

var username = await user.DisplayName.GetAsync();
Console.WriteLine($"Your new username is: {username}");
```

The IndexedProperty is very similar, however it does not require unique validation and so it does not return a boolean for the SetAsync.

```csharp
await user.Biography.SetAsync("Example");

var bio = await user.Biography.GetAsync();
```
## Queries

Any model that has an IndexedProperty or a RequiredProperty will be available to be queried.

Find all the results where user bio contains or is similar to "exam"
```csharp
var results = await QueryExecutor.Query<User>()
.Text(s => s.Biography, "exam")
.SearchAsync();
```

Find all matches where the text is exactly (with case insensitivity)
```csharp
var results = await QueryExecutor.Query<User>()
.TextExact(s => s.Biography, "Example")
.SearchAsync();
```

Return all results where user has money between 0 and 1000, and a username similar or exactly "legend"
```csharp
var results = await QueryExecutor.Query<User>()
    .Numeric(s => s.Money, 0, 1000)
    .Text(s => s.DisplayName, "legend")
    .Or()
    .TextExact(s => s.Biography, "Example")
    .SearchAsync();
```

Queries strung together are automatically processed with the AND operation.
You have the options of Or and Not for additional complex queries.

```csharp
var results = await QueryExecutor.Query<User>()
    .Numeric(s => s.Money, 0, 1000)
    .Text(s => s.DisplayName, "legend")
    .Not()
    .TextExact(s => s.Biography, "Example")
    .SearchAsync();
```

In some cases, you may need to use the query in a non-generic manner
```csharp
var documentIds = await QueryExecutor.Query()
.TextExact("propertyName", "exactValue")
.SearchAsync("indexName");
```

You also have the convienent option of "ContainsAsync"
```csharp
var foundResult = await QueryExecutor.Query()
.TextExact("propertyName", "exactValue")
.ContainsAsync("indexName");
```
or with generics

```csharp
var foundResult = await QueryExecutor.Query<User>()
.TextExact(s => s.DisplayName, "legend")
.ContainsAsync();

```

## Document Links

Document Links serve to directly tie one model to another. Here is an example:

```csharp
    public class User : IAsyncDocument
    {
        public string Id { get; set; }

        public AsyncLinkSet<Friendship> Friendships => new(this);
        public AsyncManagedLinkSet<Friendship> ManagedFriends => new(this);

        public AsyncLink<User> BestFriend => new(this);
        public ManagedLink<User> ManagedBestFriend => new(this);

        public string IndexName()
        {
            return "users";
        }
    }

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
```

```csharp
var user1 = new User { Id = "user1" };
var user2 = new User { Id = "user2" };

var friendship = new Friendship { Id = Guid.NewGuid().ToString() };
await friendship.Initiator.SetAsync(user1);
await friendship.Receiver.SetAsync(user2);

await user1.Friendships.AddAsync(friendship);
await user2.Friendships.AddAsync(friendship);

var bestFriend = await user1.BestFriend.GetAsync();
await bestFriend.Friendships.GetAsync("id");
```

- Important Notes:
Unlike standard links, ManagedLinks (e.g., ManagedBestFriend) automatically delete associated documents when the parent document is deleted.
For example, if a user is deleted, all their managed links (e.g., their ManagedBestFriend) are also deleted.
This setup simplifies handling relationships and ensures data consistency, especially when cascading deletions are necessary.


- Static Links:
Static Links are a permanent connection to another model, that is managed and deleted with the main document.
```csharp
        public StaticLink<Friendship> Friendship => new(this);
```

Their purpose is to allow accessing a nested document without actually having to load it from the server, which in some cases gives a level of additional nesting capabilities within a complex application.
```csharp
await user1.Friendship.Document.CreatedDate.GetAsync();
```
## Collections

Although it is easy to store lists and dictionaries within an AsyncProperty, I have provided more optimized methods of storing potentially large dictionaries and lists.

Store whatever you like, it's really simple.
```csharp
        public AsyncDictionary<string, CharacterModel> Characters => new(this);
        public AsyncList<string> Friends => new(this);
```

Most standard methods are available on the dictionary and list components.
```csharp
bool hasCharacter = await user.Characters.ContainsKeyAsync("characterKey");
var characters = await user.Characters.GetAsync();

var specificCharacter = await user.Characters.GetByKeyAsync("myKey");
```
## Acknowledgements
 - [StackExchangeRedis](https://github.com/StackExchange/StackExchange.Redis)
 - [RediSearchClient](https://github.com/tombatron/RediSearchClient)



## Authors

- [@BrylyMaeder](https://www.github.com/BrylyMaeder)

