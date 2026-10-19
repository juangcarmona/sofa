var builder = DistributedApplication.CreateBuilder(args);
var token = builder.AddParameter("github-token", secret: true);
builder.AddProject<Projects.SoFa_Web>("sofa")
    .WithEnvironment("GitHub__Token", token);
builder.Build().Run();
