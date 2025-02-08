// See https://aka.ms/new-console-template for more information
using AsyncRedisDocuments;
using AsyncRedisDocuments.Index;
using Sample;


RedisSingleton.Initialize("host", 13464, "password");

var user = new User
{
    Id = "myUser",
};