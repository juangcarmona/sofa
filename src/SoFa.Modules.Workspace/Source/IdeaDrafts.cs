using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record IdeaSource(long RepositoryId, long IssueId, int Number, string Url);
internal sealed record IdeaDraft(string Id, int Revision, string Repository, string Title, string Outcome, string Scope,
    string State = "draft", long? ActorId = null, long? RepositoryId = null, IdeaSource? Source = null);

internal sealed partial class ControlJournal
{
  // Same protected database, filesystem validation, bounded records and key custody as issue controls.
  internal JournalResult Ideas(string owner, IdeaDraft? replacement = null, int? expectedRevision = null) => Run(connection =>
  {
    var scope = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes("ideas:" + owner)));
    Execute(connection, null, "CREATE TABLE IF NOT EXISTS ideas(id TEXT NOT NULL,owner TEXT NOT NULL,value BLOB NOT NULL,PRIMARY KEY(id,owner));");
    using var transaction = connection.BeginTransaction(deferred: false);
    if (replacement is not null)
    {
      using var read = Command(connection, transaction, "SELECT value FROM ideas WHERE id=$id AND owner=$owner", ("$id", replacement.Id), ("$owner", scope));
      var previous = read.ExecuteScalar() as byte[];
      var draft = previous is null ? null : Unprotect(previous).Deserialize<IdeaDraft>();
      if ((draft?.Revision ?? 0) != expectedRevision) return new("conflict");
      using var count = Command(connection, transaction, "SELECT COUNT(*) FROM ideas");
      if (previous is null && (long)count.ExecuteScalar()! >= 25) return new("unavailable");
      Execute(connection, transaction, "INSERT INTO ideas VALUES($id,$owner,$value) ON CONFLICT(id,owner) DO UPDATE SET value=$value",
          ("$id", replacement.Id), ("$owner", scope), ("$value", Protect(JsonSerializer.SerializeToElement(replacement))));
    }
    using var list = Command(connection, transaction, "SELECT value FROM ideas WHERE owner=$owner ORDER BY rowid DESC", ("$owner", scope));
    using var reader = list.ExecuteReader();
    var values = new List<IdeaDraft>();
    while (reader.Read()) values.Add(Unprotect((byte[])reader[0]).Deserialize<IdeaDraft>()!);
    reader.Close(); transaction.Commit();
    return new("available", Prepared: JsonSerializer.SerializeToElement(values));
  });
}
