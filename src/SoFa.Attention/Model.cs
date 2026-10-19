namespace SoFa.Attention;

public enum Category { Checks, Approval, Blockers, Releases }
public enum Coverage { Complete, Pending, Incomplete, Unavailable, Unconfigured }

public sealed record RepositoryMapping(string Name, string? BlockerLabel = null,
    string? OwnerReviewLabel = null, string? ReleaseWorkflow = null, string? ReleaseTarget = null);

public sealed record Observation(string Id, string Title, string Url, DateTimeOffset EvidenceTime,
    string State, string Revision = "", string CurrentRevision = "", long Sequence = 0,
    string Author = "", bool ReviewRequested = false, bool OwnerReview = false,
    bool IsDraft = false, bool IsOpen = true, bool ExplicitBlock = false, string Target = "");

public sealed record CategoryEvidence(Category Category, Coverage Coverage, string Detail,
    IReadOnlyList<Observation> Observations);
public sealed record RepositoryEvidence(string Repository, IReadOnlyList<CategoryEvidence> Categories);
public sealed record Finding(Category Category, string Reason, Observation Evidence);
public sealed record CategoryResult(Category Category, Coverage Coverage, string Detail,
    IReadOnlyList<Finding> Findings, IReadOnlyList<Observation> Observations);
public sealed record RepositoryResult(string Repository, IReadOnlyList<CategoryResult> Categories);
public sealed record AttentionSnapshot(DateTimeOffset StartedAt, DateTimeOffset CompletedAt,
    IReadOnlyList<RepositoryResult> Repositories);

public interface IEvidenceSource
{
  Task<RepositoryEvidence> CollectAsync(RepositoryMapping repository, CancellationToken cancellationToken);
}

public static class AttentionRules
{
  public static CategoryResult Classify(CategoryEvidence evidence, string owner)
  {
    var observations = evidence.Observations;
    var current = observations.Where(x => evidence.Category != Category.Checks ||
        (x.Revision.Length > 0 && x.Revision == x.CurrentRevision));
    var latest = current.GroupBy(x => x.Id).Select(x => x.OrderByDescending(o => o.Sequence)
        .ThenByDescending(o => o.EvidenceTime).First()).ToArray();
    var findings = new List<Finding>();
    foreach (var item in latest)
    {
      var reason = evidence.Category switch
      {
        Category.Approval when item.IsOpen && !item.IsDraft => ApprovalReason(item, owner),
        Category.Blockers when item.IsOpen && item.ExplicitBlock => "Explicitly blocked",
        Category.Checks when Failed(item.State) => "Check: " + item.State,
        Category.Releases when item.Target.Length > 0 && Failed(item.State) => "Release: " + item.State,
        _ => null
      };
      if (reason is not null) findings.Add(new(evidence.Category, reason, item));
    }
    var coverage = evidence.Coverage;
    if (coverage == Coverage.Complete && evidence.Category is Category.Checks or Category.Releases &&
        latest.Any(x => x.State != "success" && !Failed(x.State))) coverage = Coverage.Pending;
    // Findings are not an all-clear: exact source conclusions remain visible alongside coverage.
    return new(evidence.Category, coverage, evidence.Detail, findings.AsReadOnly(), Array.AsReadOnly(latest));
  }

  private static bool Failed(string state) => state is "failure" or "error" or "timed_out" or "action_required" or "startup_failure";
  private static string? ApprovalReason(Observation item, string owner)
  {
    var reasons = new List<string>();
    if (string.Equals(item.Author, owner, StringComparison.OrdinalIgnoreCase)) reasons.Add("authored by Juan");
    if (item.ReviewRequested) reasons.Add("review requested");
    if (item.OwnerReview) reasons.Add("owner-review convention");
    return reasons.Count == 0 ? null : "Awaiting Juan’s approval: " + string.Join(", ", reasons);
  }
}
