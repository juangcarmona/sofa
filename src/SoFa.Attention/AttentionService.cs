using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SoFa.Attention;

public sealed class AttentionService(IEvidenceSource source, IReadOnlyList<RepositoryMapping> repositories,
    string owner, CancellationToken shutdown = default)
{
  public const string InstrumentationName = "SoFa.Attention";
  public static readonly ActivitySource Activities = new(InstrumentationName);
  public static readonly Meter Meter = new(InstrumentationName);
  private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("sofa.collection.duration", "s");
  private static readonly Counter<long> Outcomes = Meter.CreateCounter<long>("sofa.collection.outcomes");
  private readonly object gate = new();
  private Task<AttentionSnapshot>? refresh;
  private AttentionSnapshot? current;
  private string? lastError;
  public AttentionSnapshot? Current => Volatile.Read(ref current);
  public string? LastError => Volatile.Read(ref lastError);
  public bool IsRefreshing { get { lock (gate) return refresh is { IsCompleted: false }; } }

  public Task<AttentionSnapshot> RefreshAsync()
  {
    lock (gate)
    {
      if (refresh is { IsCompleted: false }) return refresh;
      refresh = RefreshCoreAsync();
      return refresh;
    }
  }

  private async Task<AttentionSnapshot> RefreshCoreAsync()
  {
    using var activity = Activities.StartActivity("attention.refresh");
    var start = DateTimeOffset.UtcNow;
    try
    {
      // Five configured repositories, at most two collecting concurrently.
      using var slots = new SemaphoreSlim(2);
      var results = await Task.WhenAll(repositories.Select(async repository =>
      {
        await slots.WaitAsync(shutdown);
        try { return await CollectAsync(repository); }
        finally { slots.Release(); }
      }));
      var snapshot = new AttentionSnapshot(start, DateTimeOffset.UtcNow, Array.AsReadOnly(results));
      Volatile.Write(ref current, snapshot);
      Volatile.Write(ref lastError, null);
      activity?.SetStatus(ActivityStatusCode.Ok);
      return snapshot;
    }
    catch (Exception) when (!shutdown.IsCancellationRequested)
    {
      Volatile.Write(ref lastError, "Refresh failed. The last completed snapshot is shown with its original timestamps.");
      activity?.SetStatus(ActivityStatusCode.Error, "refresh_failed");
      throw new InvalidOperationException("Refresh failed; no new snapshot was published.");
    }
  }

  private async Task<RepositoryResult> CollectAsync(RepositoryMapping mapping)
  {
    using var activity = Activities.StartActivity("repository.collect");
    activity?.SetTag("repository", mapping.Name);
    var timer = Stopwatch.StartNew();
    RepositoryEvidence evidence;
    try
    {
      using var timeout = CancellationTokenSource.CreateLinkedTokenSource(shutdown);
      timeout.CancelAfter(TimeSpan.FromSeconds(90));
      evidence = await source.CollectAsync(mapping, timeout.Token);
    }
    catch (Exception) when (!shutdown.IsCancellationRequested)
    {
      activity?.SetStatus(ActivityStatusCode.Error, "collection_unavailable");
      evidence = new(mapping.Name, Array.AsReadOnly(Enum.GetValues<Category>()
          .Select(c => new CategoryEvidence(c, Coverage.Unavailable, "Collection unavailable or timed out.", [])).ToArray()));
    }
    var results = evidence.Categories.Select(c => AttentionRules.Classify(c, owner)).ToArray();
    foreach (var result in results)
    {
      activity?.SetTag("coverage." + result.Category.ToString().ToLowerInvariant(), result.Coverage.ToString());
      Outcomes.Add(1, new("repository", mapping.Name), new("category", result.Category.ToString()),
          new("outcome", result.Coverage.ToString()));
    }
    Duration.Record(timer.Elapsed.TotalSeconds, new KeyValuePair<string, object?>("repository", mapping.Name));
    return new(mapping.Name, Array.AsReadOnly(results));
  }
}
