using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using SoFa.Attention;
using SoFa.GitHub;
using SoFa.Web.Pages;
using Xunit;

namespace SoFa.Tests;

public class WebTests
{
  [Fact]
  public void ConfigurationDoesNotOverrideAspireAssignedEndpoint()
  {
    using var factory = new Factory(new AttentionTests.StubSource((m, _) => Task.FromResult(AttentionTests.Empty(m.Name))));
    var config = factory.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
    Assert.Null(config["Urls"]);
    Assert.Equal("localhost;127.0.0.1", config["AllowedHosts"]);
  }

  [Theory]
  [InlineData("javascript:alert(1)")]
  [InlineData("https://github.com.evil.test/secret")]
  [InlineData("https://name@github.com/a/b")]
  [InlineData("http://github.com/a/b")]
  public void UntrustedLinksAreNotNavigable(string url) => Assert.Null(IndexModel.SafeLink(url));

  [Fact]
  public async Task PageRefreshRequiresAntiforgeryAndEscapesEvidence()
  {
    var source = new AttentionTests.StubSource((m, _) => Task.FromResult(new RepositoryEvidence(m.Name,
        [new(Category.Approval, Coverage.Complete, "Inspected", [new("1", "<script>alert(1)</script>", "javascript:alert(1)", DateTimeOffset.UtcNow, "open", Author: "juangcarmona")]),
             new(Category.Blockers, Coverage.Unconfigured, "No mapping", []),
             new(Category.Releases, Coverage.Incomplete, "Target unknown", [])])));
    using var factory = new Factory(source);
    using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
    var empty = await client.GetStringAsync("/");
    Assert.Contains("No collection has occurred", empty);
    Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/?handler=Refresh", new FormUrlEncodedContent([]))).StatusCode);
    var response = await Refresh(client, empty);
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    var html = await client.GetStringAsync("/");
    Assert.Contains("Awaiting Juan", html);
    Assert.Contains("&lt;script&gt;", html);
    Assert.DoesNotContain("<script>", html);
    Assert.DoesNotContain("href=\"javascript:", html);
    Assert.Contains("Unconfigured", html);
    Assert.Contains("Incomplete", html);
    Assert.Contains("snapshot, not a live feed", html);
  }

  [Fact]
  public async Task DistinctiveCredentialNeverAppearsInOutputLogsOrExportedTelemetry()
  {
    const string marker = "SOFA_SYNTHETIC_CREDENTIAL_8e51_for_negative_test";
    var exporter = new TraceCollector();
    using var provider = Sdk.CreateTracerProviderBuilder().SetSampler(new AlwaysOnSampler()).AddSource(AttentionService.InstrumentationName)
        .AddProcessor(new SimpleActivityExportProcessor(exporter)).Build();
    var logs = new LogCollector();
    var http = new HttpClient(new GitHubTests.Handler(request =>
    {
      Assert.Equal(marker, request.Headers.Authorization!.Parameter);
      return GitHubTests.Json(new { message = marker }, HttpStatusCode.Forbidden);
    }))
    { BaseAddress = new Uri("https://api.github.com/") };
    http.DefaultRequestHeaders.Authorization = new("Bearer", marker);
    using var factory = new Factory(new GitHubEvidenceSource(http, "juangcarmona"), logs);
    using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
    await Refresh(client, await client.GetStringAsync("/"));
    var html = await client.GetStringAsync("/");
    provider.ForceFlush();
    Assert.Contains("Unavailable", html);
    Assert.NotEmpty(exporter.Exported);
    Assert.Contains(exporter.Exported, x => x.Contains("AccessDeniedOrRateLimited"));
    Assert.Contains(logs.Messages, x => x.Contains("refresh completed"));
    Assert.DoesNotContain(marker, html);
    Assert.DoesNotContain(marker, string.Join("\n", logs.Messages));
    Assert.DoesNotContain(marker, string.Join("\n", exporter.Exported));
    Assert.Equal(5, exporter.Exported.Count(x => x.Contains("repository.collect")));
  }

  internal static Task<HttpResponseMessage> Refresh(HttpClient client, string html)
  {
    var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
    Assert.NotEmpty(token);
    return client.PostAsync("/?handler=Refresh", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token) }));
  }

  private sealed class Factory(IEvidenceSource source, ILoggerProvider? logs = null) : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Testing");
      builder.ConfigureServices(services =>
      {
        services.RemoveAll<IEvidenceSource>();
        services.AddSingleton(source);
        if (logs is not null) services.AddSingleton(logs);
      });
    }
  }
  private sealed class TraceCollector : BaseExporter<Activity>
  {
    public ConcurrentBag<string> Exported { get; } = [];
    public override ExportResult Export(in Batch<Activity> batch)
    {
      foreach (var activity in batch)
        Exported.Add(activity.OperationName + JsonSerializer.Serialize(activity.TagObjects) + activity.StatusDescription);
      return ExportResult.Success;
    }
  }
  private sealed class LogCollector : ILoggerProvider
  {
    public ConcurrentBag<string> Messages { get; } = [];
    public ILogger CreateLogger(string categoryName) => new Capture(Messages);
    public void Dispose() { }
    private sealed class Capture(ConcurrentBag<string> messages) : ILogger
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
      public bool IsEnabled(LogLevel logLevel) => true;
      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => messages.Add(formatter(state, exception));
    }
  }
}
