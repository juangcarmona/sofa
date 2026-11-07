using Microsoft.Extensions.Configuration;

namespace JuanGCarmona.SoFa.AppHost;

public static class Program
{
  public static void Main(string[] args)
  {
    var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
    { Args = args, DisableDashboard = true });
    var file = Environment.GetEnvironmentVariable("SOFA_CONFIG_FILE");
    if (string.IsNullOrEmpty(file) || !Path.IsPathFullyQualified(file))
      throw new InvalidOperationException("Select an absolute private configuration file for this workspace.");
    builder.Configuration.AddJsonFile(file, optional: false, reloadOnChange: false).AddEnvironmentVariables();
    WorkspaceComposition.Add(builder, file);
    builder.Build().Run();
  }
}
