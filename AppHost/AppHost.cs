var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("mongodb");

var wiki = builder.AddProject<Projects.Wiki>("wiki")
    .WithReference(mongodb)
    .WaitFor(mongodb);

builder.Build().Run();