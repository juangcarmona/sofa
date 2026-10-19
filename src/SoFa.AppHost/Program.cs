using System.Net;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(Path.Combine(builder.AppHostDirectory, "appsettings.Local.json"), optional: true);
var token = builder.AddParameter("github-token", secret: true);
var sofa = builder.AddProject<Projects.SoFa_Web>("sofa")
    .WithEnvironment("GitHub__Token", token);

// Local deployment choice: expose only the explicitly configured interface.
var lanAddress = builder.Configuration["SoFa:LanAddress"];
if (!string.IsNullOrWhiteSpace(lanAddress))
{
  if (!IPAddress.TryParse(lanAddress, out var address) ||
      address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
      IPAddress.Any.Equals(address))
    throw new InvalidOperationException("SoFa:LanAddress must be a specific IPv4 interface address.");

  sofa.WithEndpoint("http", endpoint =>
  {
    endpoint.IsProxied = false;
    endpoint.Port = 5168;
    endpoint.TargetPort = 5168;
    endpoint.TargetHost = address.ToString();
  }).WithEnvironment("AllowedHosts", "localhost;127.0.0.1;" + address);
}

builder.Build().Run();
