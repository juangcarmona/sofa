using System.Globalization;
using System.Net;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using JuanGCarmona.SoFa.Modules.Workspace;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;
using JuanGCarmona.SoFa.Modules.Workspace.Source;
using Microsoft.AspNetCore.Authentication;

namespace SoFa.Web.Tests;

[SupportedOSPlatform("linux")]
public sealed class IdeaCommandsTests
{
  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task CreateRetainsIdentityAndLostResponseReconcilesAcrossRestartWithoutResend(bool lost)
  {
    using var f = new Fixture(); var session = await f.Session();
    var draft = await f.Save(session);
    Assert.Empty(f.Handler.Writes);
    var review = await f.Commands.Handle(session, new("review", draft.Id, draft.Revision), CancellationToken.None);
    draft = Assert.Single(review.Ideas);
    Assert.Equal("reviewed", draft.State); Assert.Contains("Intended outcome", review.Preview);
    Assert.Empty(f.Handler.Writes);
    f.Handler.LoseResponse = lost;
    var sent = await f.Commands.Handle(session, new("create", draft.Id, draft.Revision), CancellationToken.None);
    Assert.Equal(lost ? "uncertain" : "created", Assert.Single(sent.Ideas).State);
    Assert.Single(f.Handler.Writes);
    f.Restart(); draft = Assert.Single(f.Commands.List().Ideas);
    if (lost)
    {
      var refused = await f.Commands.Handle(session, new("create", draft.Id, draft.Revision), CancellationToken.None);
      Assert.Equal("conflict", refused.Availability);
      var reconciled = await f.Commands.Handle(session, new("reconcile", draft.Id, draft.Revision), CancellationToken.None);
      draft = Assert.Single(reconciled.Ideas);
    }
    Assert.Equal(70, draft.Source!.IssueId); Assert.Equal(7, draft.Source.Number);
    Assert.Equal(22, draft.Source.RepositoryId); Assert.Equal("created", draft.State);
    await f.Commands.Handle(session, new("create", draft.Id, draft.Revision), CancellationToken.None);
    Assert.Single(f.Handler.Writes);
  }

  [Fact]
  public async Task LinkAndAbandonNeverWriteAndDraftSurvivesRefresh()
  {
    using var f = new Fixture(); var session = await f.Session();
    var draft = await f.Save(session); f.Restart();
    Assert.Equal("Useful idea", Assert.Single(f.Commands.List().Ideas).Title);
    f.Handler.CanWrite = false;
    var bypass = await f.Commands.Handle(session, new("link", draft.Id, draft.Revision, Number: 7), CancellationToken.None);
    Assert.Equal("conflict", bypass.Availability);
    Assert.Equal("draft", Assert.Single(bypass.Ideas).State);
    Assert.Equal(0, f.Handler.IssueReads);
    var review = await f.Commands.Handle(session, new("review", draft.Id, draft.Revision), CancellationToken.None);
    draft = Assert.Single(review.Ideas);
    Assert.Equal("reviewed", draft.State);
    Assert.Contains(review.Related!, value => value.Number == 7);
    var linked = await f.Commands.Handle(session, new("link", draft.Id, draft.Revision, Number: 7), CancellationToken.None);
    Assert.Equal("linked", Assert.Single(linked.Ideas).State);
    Assert.Equal(70, Assert.Single(linked.Ideas).Source!.IssueId);
    draft = await f.Save(session);
    var abandoned = await f.Commands.Handle(session, new("abandon", draft.Id, draft.Revision), CancellationToken.None);
    Assert.Equal("abandoned", abandoned.Ideas.Single(value => value.Id == draft.Id).State);
    Assert.Empty(f.Handler.Writes);
  }

  [Fact]
  public async Task MissingPermissionDuplicateAndStaleRevisionBlockCreation()
  {
    using var f = new Fixture(); var session = await f.Session(); var draft = await f.Save(session);
    f.Handler.CanWrite = false;
    draft = Assert.Single((await f.Commands.Handle(session, new("review", draft.Id, draft.Revision), CancellationToken.None)).Ideas);
    Assert.Equal("reviewed", draft.State);
    Assert.Equal("denied", (await f.Commands.Handle(session, new("create", draft.Id, draft.Revision), CancellationToken.None)).Availability);
    Assert.Empty(f.Handler.Writes);
    f.Handler.CanWrite = true;
    draft = Assert.Single((await f.Commands.Handle(session, new("review", draft.Id, draft.Revision), CancellationToken.None)).Ideas);
    f.Handler.Equivalent = true;
    Assert.Equal("conflict", (await f.Commands.Handle(session, new("create", draft.Id, draft.Revision), CancellationToken.None)).Availability);
    Assert.Equal("conflict", (await f.Commands.Handle(session, new("abandon", draft.Id, draft.Revision - 1), CancellationToken.None)).Availability);
    Assert.Empty(f.Handler.Writes);
  }

  [Fact]
  public async Task ConcurrentConfirmationsSendOnceAndUnmatchedReconciliationNeverRetries()
  {
    using var f = new Fixture(); var session = await f.Session(); var draft = await f.Save(session);
    draft = Assert.Single((await f.Commands.Handle(session, new("review", draft.Id, draft.Revision), CancellationToken.None)).Ideas);
    f.Handler.LoseResponse = true;
    await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => f.Commands.Handle(session, new("create", draft.Id, draft.Revision), CancellationToken.None)));
    Assert.Single(f.Handler.Writes);
    draft = Assert.Single(f.Commands.List().Ideas); f.Handler.HideCreated = true;
    var unknown = await f.Commands.Handle(session, new("reconcile", draft.Id, draft.Revision), CancellationToken.None);
    Assert.Equal("conflict", unknown.Availability); Assert.Equal("uncertain", Assert.Single(unknown.Ideas).State);
    Assert.Single(f.Handler.Writes);
  }

  private sealed class Fixture : IDisposable
  {
    private readonly TestClock clock = new();
    private readonly string root = Path.Combine(Path.GetTempPath(), "sofa-idea-" + Guid.NewGuid().ToString("N"));
    private readonly GitHubTransport transport;
    private readonly OwnerSessions sessions;
    private readonly WorkspaceOptions settings;
    private readonly SourceControlAccess source;
    private ControlJournal journal;
    internal Handler Handler { get; } = new();
    internal IdeaCommands Commands { get; private set; }
    internal Fixture()
    {
      Directory.CreateDirectory(root, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
      foreach (var name in new[] { "state", "keys" }) Directory.CreateDirectory(root + "/" + name, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
      sessions = new(OwnerAdmissionTests.Profile(), clock); transport = new(Handler);
      settings = new() { Origin = new("https://workspace.invalid"), IdentityMode = "Development", Identity = OwnerAdmissionTests.Profile(), Repositories = ["example/repository"], GitHubAppId = 9001 };
      var authorization = new GitHubAuthorization(settings, sessions, transport, clock);
      var access = new RepositoryAccess(settings, sessions, authorization, transport, clock);
      source = new(settings, sessions, authorization, access, transport);
      journal = new(root + "/state", root + "/keys"); Commands = new(settings, source, journal, transport);
    }
    internal void Restart() { journal.Dispose(); journal = new(root + "/state", root + "/keys"); Commands = new(settings, source, journal, transport); }
    internal async Task<IdeaDraft> Save(OwnerSession session) => (await Commands.Handle(session,
        new("save", Repository: "example/repository", Title: "Useful idea", Outcome: "An intended result", Scope: "Bounded scope"), CancellationToken.None)).Ideas[0];
    internal async Task<OwnerSession> Session()
    {
      var principal = OwnerAdmissionTests.Principal(authenticationTime: clock.GetUtcNow().ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
      var session = sessions.Get(await sessions.StoreAsync(new AuthenticationTicket(principal, "owner")))!;
      session.GitHub.Current = new(71, "verified-user", new("isolated-source-access", "isolated-source-refresh", clock.GetUtcNow().AddHours(1), clock.GetUtcNow().AddHours(4)), clock.GetUtcNow()); return session;
    }
    public void Dispose() { journal.Dispose(); transport.Dispose(); Directory.Delete(root, recursive: true); }
  }

  private sealed class Handler : HttpMessageHandler
  {
    internal bool CanWrite { get; set; } = true;
    internal bool LoseResponse { get; set; }
    internal bool Equivalent { get; set; }
    internal bool HideCreated { get; set; }
    internal int IssueReads { get; private set; }
    internal List<string> Writes { get; } = [];
    private string body = "Existing issue";
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Assert.Equal("api.github.com", request.RequestUri!.Host);
      var path = request.RequestUri.AbsolutePath;
      object Issue() => new
      {
        id = 70,
        number = 7,
        title = Equivalent ? " useful   idea " : "Useful idea follow-up",
        body,
        html_url = "https://github.com/example/repository/issues/7",
        user = new { id = 71 }
      };
      if (request.Method == HttpMethod.Post)
      {
        Assert.Equal("/repos/example/repository/issues", path);
        var posted = await request.Content!.ReadAsStringAsync(cancellationToken);
        using var json = JsonDocument.Parse(posted); body = json.RootElement.GetProperty("body").GetString()!;
        Writes.Add(posted);
        if (LoseResponse) throw new HttpRequestException("Fixture lost response");
        return Reply(Issue(), HttpStatusCode.Created);
      }
      if (path == "/repos/example/repository/issues/7") IssueReads++;
      return Reply(path switch
      {
        "/user" => new { id = 71, login = "verified-user", type = "User" },
        "/repos/example/repository" => new { id = 22, full_name = "example/repository", permissions = new { pull = true, push = CanWrite, maintain = false, admin = false } },
        "/user/installations" => new { total_count = 1, installations = new[] { new { id = 9, app_id = 9001, account = new { login = "example" }, permissions = new { metadata = "read", issues = "write" } } } },
        "/user/installations/9/repositories" => new { total_count = 1, repositories = new[] { new { id = 22, full_name = "example/repository" } } },
        "/repos/example/repository/issues/7" => Issue(),
        "/repos/example/repository/issues" => Equivalent || Writes.Count == 0 || !HideCreated ? new[] { Issue() } : Array.Empty<object>(),
        _ => throw new InvalidOperationException("Unexpected source read: " + path)
      });
    }
    private static HttpResponseMessage Reply(object data, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json") };
  }
}
