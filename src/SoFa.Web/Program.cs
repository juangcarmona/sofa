using System.Net.Http.Headers;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SoFa.Attention;
using SoFa.GitHub;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options => options.Cookie.SameSite = SameSiteMode.Strict);
builder.Services.AddSingleton<IReadOnlyList<RepositoryMapping>>(_ =>
{
  var mappings = builder.Configuration.GetSection("Repositories").Get<RepositoryMapping[]>() ?? [];
  var expected = new[] { "arcshape", "productshape", "domusmind", "raccord", "sofa" }.Select(n => "juangcarmona/" + n).Order().ToArray();
  if (!mappings.Select(m => m.Name).Order().SequenceEqual(expected))
    throw new InvalidOperationException("Configure exactly the initial five repositories, without duplicates.");
  return Array.AsReadOnly(mappings);
});
builder.Services.AddSingleton(_ =>
{
  var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
  {
    BaseAddress = new Uri("https://api.github.com/"),
    Timeout = TimeSpan.FromSeconds(12)
  };
  client.DefaultRequestHeaders.UserAgent.ParseAdd("SoFa/0.1");
  client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
  client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
  var token = builder.Configuration["GitHub:Token"];
  if (!string.IsNullOrWhiteSpace(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
  return client;
});
builder.Services.AddSingleton<IEvidenceSource>(sp => new GitHubEvidenceSource(sp.GetRequiredService<HttpClient>(), "juangcarmona"));
builder.Services.AddSingleton(sp => new AttentionService(sp.GetRequiredService<IEvidenceSource>(),
    sp.GetRequiredService<IReadOnlyList<RepositoryMapping>>(), "juangcarmona",
    sp.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping));
var telemetry = builder.Services.AddOpenTelemetry().ConfigureResource(r => r.AddService("sofa"))
    .WithTracing(t => t.SetSampler(new AlwaysOnSampler()).AddSource(AttentionService.InstrumentationName))
    .WithMetrics(m => m.AddMeter(AttentionService.InstrumentationName));
if (!string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
  telemetry.WithTracing(t => t.AddOtlpExporter()).WithMetrics(m => m.AddOtlpExporter());
  builder.Logging.AddOpenTelemetry(o => o.AddOtlpExporter());
}
var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");
app.Use(async (context, next) =>
{
  context.Response.Headers.ContentSecurityPolicy = "default-src 'self'; style-src 'self'; script-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";
  context.Response.Headers.XContentTypeOptions = "nosniff";
  context.Response.Headers.CacheControl = "no-store";
  context.Response.Headers["Referrer-Policy"] = "no-referrer";
  await next();
});
app.UseStaticFiles();
app.MapRazorPages();
app.Run();

public partial class Program { }
