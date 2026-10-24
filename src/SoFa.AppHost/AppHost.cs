var builder = DistributedApplication.CreateBuilder(args);
var database = builder.AddPostgres("database").WithDataVolume().AddDatabase("sofa");
builder.AddProject<Projects.SoFa_Runtime>("supervisor")
    .WithReference(database).WaitFor(database);
builder.Build().Run();
