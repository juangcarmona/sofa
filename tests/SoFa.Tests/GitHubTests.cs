using System.Net;
using System.Text.Json;
using SoFa.Attention;
using SoFa.GitHub;
using Xunit;

namespace SoFa.Tests;

public class GitHubTests
{
  internal sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> handle) : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Assert.Equal(HttpMethod.Get, request.Method);
      return Task.FromResult(handle(request));
    }
  }
  internal static HttpResponseMessage Json(object value, HttpStatusCode status = HttpStatusCode.OK) =>
      new(status) { Content = new StringContent(JsonSerializer.Serialize(value)) };
  private static object Pr(string sha = "head") => new
  {
    number = 1,
    title = "A PR",
    html_url = "https://github.com/a/b/pull/1",
    updated_at = "2026-10-19T10:00:00Z",
    state = "open",
    draft = false,
    user = new { login = "juangcarmona" },
    requested_reviewers = Array.Empty<object>(),
    labels = Array.Empty<object>(),
    head = new { sha },
    merge_commit_sha = (string?)null
  };
  internal static GitHubEvidenceSource Source(Func<HttpRequestMessage, HttpResponseMessage> handler) =>
      new(new HttpClient(new Handler(handler)) { BaseAddress = new Uri("https://api.github.com/") }, "juangcarmona");

  [Fact]
  public async Task AmbiguousPublicationTargetDoesNotUseDispatchedRevision()
  {
    var source = Source(r => r.RequestUri!.AbsolutePath.Contains("/runs") ?
        Json(new { workflow_runs = new[] { new { id = 1, head_branch = "main", head_sha = "dispatched-not-published", @event = "workflow_dispatch", conclusion = "failure" } } }) : Json(Array.Empty<object>()));
    var result = await source.CollectAsync(new("a/b", ReleaseWorkflow: "release.yml", ReleaseTarget: "unknown"), default);
    var release = result.Categories.Single(c => c.Category == Category.Releases);
    Assert.Equal(Coverage.Incomplete, release.Coverage);
    Assert.Empty(release.Observations);
  }

  [Fact]
  public async Task NewestTagUsesLatestRunAttemptAndIgnoresOldFailedTargets()
  {
    var source = Source(r =>
    {
      var path = r.RequestUri!.PathAndQuery;
      if (path.Contains("workflows/"))
      {
        Assert.Contains("per_page=1", path);
        return Json(new { workflow_runs = new[] { new { id = 9, head_branch = "v2", head_sha = "sha2", @event = "release" } } });
      }
      if (path.Contains("git/ref")) return Json(new { @object = new { type = "commit", sha = "sha2" } });
      if (path.Contains("actions/runs/9")) return Json(new { id = 9, head_branch = "v2", head_sha = "sha2", status = "completed", conclusion = "success", run_attempt = 2, html_url = "https://github.com/a/b/actions/runs/9", updated_at = "2026-10-19T10:00:00Z" });
      return Json(Array.Empty<object>());
    });
    var evidence = await source.CollectAsync(new("a/b", ReleaseWorkflow: "publish.yml", ReleaseTarget: "tag"), default);
    var release = evidence.Categories.Single(c => c.Category == Category.Releases);
    Assert.Equal("v2", release.Observations[0].Target);
    Assert.Equal(2, release.Observations[0].Sequence);
    Assert.Empty(AttentionRules.Classify(release, "juangcarmona").Findings);
  }

  [Fact]
  public async Task ChangedHeadCannotPublishOldFailedChecks()
  {
    int reads = 0;
    var source = Source(r =>
    {
      var path = r.RequestUri!.AbsolutePath;
      if (path.EndsWith("/pulls")) return Json(new[] { Pr() });
      if (path.EndsWith("/pulls/1")) return Json(Pr(++reads == 1 ? "head" : "changed"));
      if (path.EndsWith("/check-runs")) return Json(new { check_runs = new[] { new { id = 10, name = "build", status = "completed", conclusion = "failure", head_sha = "head", app = new { id = 1 }, html_url = "https://github.com/a/b/actions/runs/10", completed_at = "2026-10-19T10:00:00Z" } } });
      return Json(Array.Empty<object>());
    });
    var evidence = await source.CollectAsync(new("a/b"), default);
    var checks = evidence.Categories.Single(c => c.Category == Category.Checks);
    Assert.Equal(Coverage.Incomplete, checks.Coverage);
    Assert.Empty(checks.Observations);
    Assert.Single(AttentionRules.Classify(evidence.Categories.Single(c => c.Category == Category.Approval), "juangcarmona").Findings);
  }

  [Fact]
  public async Task PaginationCapIsIncompleteAndMissingMappingsAreNotClear()
  {
    var source = Source(_ => Json(Enumerable.Range(1, 100).Select(_ => Pr()).ToArray()));
    var evidence = await source.CollectAsync(new("a/b"), default);
    Assert.Equal(Coverage.Incomplete, evidence.Categories.Single(c => c.Category == Category.Approval).Coverage);
    Assert.Equal(Coverage.Unconfigured, evidence.Categories.Single(c => c.Category == Category.Blockers).Coverage);
    Assert.Equal(Coverage.Unconfigured, evidence.Categories.Single(c => c.Category == Category.Releases).Coverage);
  }

  [Fact]
  public async Task RateLimitIsExplicitAndErrorBodyIsNeverReturned()
  {
    var source = Source(_ => Json(new { message = "credential-marker" }, HttpStatusCode.Forbidden));
    var evidence = await source.CollectAsync(new("a/b"), default);
    Assert.Equal(Coverage.Unavailable, evidence.Categories.Single(c => c.Category == Category.Approval).Coverage);
    Assert.DoesNotContain("credential-marker", JsonSerializer.Serialize(evidence));
  }

  [Fact]
  public async Task TransientFailuresHaveBoundedRetry()
  {
    int calls = 0;
    var source = Source(_ => { calls++; return Json(new { }, HttpStatusCode.ServiceUnavailable); });
    await source.CollectAsync(new("a/b"), default);
    Assert.Equal(4, calls); // Two attempts for checks, two for approval; unmapped categories make no requests.
  }
}
