using SoFa.Attention;
using Xunit;

namespace SoFa.Tests;

public class AttentionTests
{
  private static Observation Item(string state = "open") => new("1", "Title", "https://github.com/a/b/pull/1", DateTimeOffset.UtcNow, state, Author: "juangcarmona");
  private static CategoryResult Classify(Category category, params Observation[] observations) =>
      AttentionRules.Classify(new(category, Coverage.Complete, "fixture", observations), "juangcarmona");

  [Fact]
  public void AuthoredPrWithoutRequestAwaitsJuanAndSignalsDeduplicate()
  {
    var item = Item();
    Assert.Single(Classify(Category.Approval, item).Findings);
    var result = Classify(Category.Approval, item with { ReviewRequested = true, OwnerReview = true });
    Assert.Single(result.Findings);
    Assert.Contains("authored by Juan", result.Findings[0].Reason);
    Assert.Contains("review requested", result.Findings[0].Reason);
  }

  [Theory]
  [InlineData(true, true)]
  [InlineData(false, false)]
  public void DraftOrClosedPrNeverAwaitsApproval(bool draft, bool open) =>
      Assert.Empty(Classify(Category.Approval, Item() with { IsDraft = draft, IsOpen = open, ReviewRequested = true }).Findings);

  [Fact]
  public void MissingApprovalAloneDoesNotAssignAnotherAuthorsPr() =>
      Assert.Empty(Classify(Category.Approval, Item() with { Author = "someone-else" }).Findings);

  [Fact]
  public void OldRevisionAndSupersededAttemptAreExcluded()
  {
    var old = Item("failure") with { Revision = "old", CurrentRevision = "new" };
    Assert.Empty(Classify(Category.Checks, old).Findings);
    var failed = old with { Revision = "new", Sequence = 1 };
    var passed = failed with { State = "success", Sequence = 2 };
    Assert.Empty(Classify(Category.Checks, failed, passed).Findings);
    Assert.Single(Classify(Category.Checks, failed).Findings);
  }

  [Theory]
  [InlineData("queued")]
  [InlineData("in_progress")]
  [InlineData("cancelled")]
  [InlineData("skipped")]
  [InlineData("neutral")]
  public void NonSuccessConclusionsArePreserved(string state)
  {
    var result = Classify(Category.Checks, Item(state) with { Revision = "abc", CurrentRevision = "abc" });
    Assert.Empty(result.Findings);
    Assert.Equal(Coverage.Pending, result.Coverage);
    Assert.Equal(state, result.Observations[0].State);
  }

  [Fact]
  public void BlockingNeedsExplicitOpenSignal()
  {
    Assert.Empty(Classify(Category.Blockers, Item()).Findings);
    Assert.Single(Classify(Category.Blockers, Item() with { ExplicitBlock = true }).Findings);
    Assert.Empty(Classify(Category.Blockers, Item() with { ExplicitBlock = true, IsOpen = false }).Findings);
  }

  [Fact]
  public async Task ConcurrentRefreshIsSingleFlightAndPublishesAtomically()
  {
    var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var calls = 0;
    var source = new StubSource(async (mapping, token) =>
    {
      Interlocked.Increment(ref calls);
      await signal.Task.WaitAsync(token);
      return Empty(mapping.Name);
    });
    var service = new AttentionService(source, [new("a/b")], "juan");
    Assert.Null(service.Current);
    var first = service.RefreshAsync();
    var second = service.RefreshAsync();
    Assert.Same(first, second);
    Assert.True(service.IsRefreshing);
    Assert.Null(service.Current);
    signal.SetResult();
    var snapshot = await first;
    Assert.Same(snapshot, service.Current);
    Assert.Equal(1, calls);
    Assert.Null(new AttentionService(source, [new("a/b")], "juan").Current);
  }

  [Fact]
  public async Task OneRepositoryFailureCannotSuppressOthers()
  {
    var service = new AttentionService(new StubSource((m, _) => m.Name == "bad" ?
        throw new HttpRequestException("sensitive source data") : Task.FromResult(Empty(m.Name))),
        [new("good"), new("bad")], "juan");
    var snapshot = await service.RefreshAsync();
    Assert.Equal(Coverage.Complete, snapshot.Repositories[0].Categories[0].Coverage);
    Assert.All(snapshot.Repositories[1].Categories, c => Assert.Equal(Coverage.Unavailable, c.Coverage));
  }

  internal static RepositoryEvidence Empty(string repo) => new(repo,
      Enum.GetValues<Category>().Select(c => new CategoryEvidence(c, Coverage.Complete, "Inspected", [])).ToArray());
  internal sealed class StubSource(Func<RepositoryMapping, CancellationToken, Task<RepositoryEvidence>> collect) : IEvidenceSource
  {
    public Task<RepositoryEvidence> CollectAsync(RepositoryMapping repository, CancellationToken cancellationToken) => collect(repository, cancellationToken);
  }
}
