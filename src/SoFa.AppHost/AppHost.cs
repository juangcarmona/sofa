// Repository workflows and the independent runtime own execution (ADR-0006).
// The observer must not start a supervisor or an execution database.
var builder = DistributedApplication.CreateBuilder(args);
builder.Build().Run();
