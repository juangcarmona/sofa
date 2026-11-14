using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.Win32.SafeHandles;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record JournalIdentity(string Owner, long ActorId, long RepositoryId, long IssueId)
{
  public override string ToString() => "JournalIdentity (private owner withheld)";
}
internal sealed record JournalResult(string State, string? Id = null, JsonElement? Prepared = null, JsonElement? Receipt = null)
{
  public override string ToString() => "JournalResult (private request withheld)";
}

// Claims are irreversible transmission tombstones. A restored/prepared row is reconciliation-only.
internal sealed partial class ControlJournal : IDisposable
{
  private PrivateJournalDirectory? storage, keys;
  private SafeFileHandle? database;
  private IDataProtector? protector;
  private string? journalId;
  private bool ready;
  private readonly object keySync = new();
  private readonly HashSet<string> keyNames = new(StringComparer.Ordinal);
  private readonly Dictionary<string, string> keyDigests = new(StringComparer.Ordinal);
  private const string Database = "intents.sqlite", Marker = ".sofa-journal-v1";

  internal ControlJournal(string? directory, string? keyDirectory)
  {
    try
    {
      if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(keyDirectory)) return;
      if (directory == keyDirectory || directory.StartsWith(keyDirectory + "/", StringComparison.Ordinal)
          || keyDirectory.StartsWith(directory + "/", StringComparison.Ordinal)) throw new IOException("Separate journal and key directories are required.");
      storage = new(directory); keys = new(keyDirectory);
      var existing = Directory.GetFileSystemEntries(storage.Path);
      var keyEntries = Directory.GetFileSystemEntries(keys.Path);
      var fresh = existing.Length == 0 && keyEntries.Length == 0;
      if (fresh)
      {
        journalId = Guid.NewGuid().ToString("N");
        using var marker = new FileStream(keys.File(Marker, create: true), FileAccess.Write);
        marker.Write(Encoding.ASCII.GetBytes(journalId)); marker.Flush(flushToDisk: true);
        keys.Sync();
      }
      else
      {
        using var marker = new FileStream(keys.File(Marker), FileAccess.Read);
        if (marker.Length != 32) throw new IOException("Journal binding is missing.");
        using var bytes = new MemoryStream(); marker.CopyTo(bytes);
        if (bytes.Length != 32 || !Guid.TryParseExact(Encoding.ASCII.GetString(bytes.ToArray()), "N", out _)) throw new IOException("Journal binding is missing.");
        journalId = Encoding.ASCII.GetString(bytes.ToArray());
      }
      database = storage.File(Database, create: fresh);
      storage.Sync();
      CheckFiles();
      protector = DataProtectionProvider.Create(new DirectoryInfo(keys.Path), options => options.SetApplicationName("SoFa.IssueControls.v1"))
          .CreateProtector("SoFa.IssueControls.PreparedAndReceipt.v1", journalId);
      using var connection = Connect();
      if (fresh)
      {
        Execute(connection, null, "PRAGMA page_size=4096; PRAGMA journal_mode=WAL; PRAGMA synchronous=FULL;");
        var canary = Protect(JsonSerializer.SerializeToElement(new { journalId }));
        using var transaction = connection.BeginTransaction(deferred: false);
        Execute(connection, transaction, "CREATE TABLE metadata(version INTEGER NOT NULL, binding BLOB NOT NULL); CREATE TABLE intents(id TEXT PRIMARY KEY, key TEXT NOT NULL UNIQUE, digest TEXT NOT NULL, scope TEXT NOT NULL, action TEXT NOT NULL, sdk TEXT NOT NULL, state TEXT NOT NULL, prepared BLOB NOT NULL, receipt BLOB, created TEXT NOT NULL, terminal INTEGER NOT NULL CHECK(terminal IN(0,1))); CREATE INDEX intents_scope ON intents(scope,terminal); INSERT INTO metadata VALUES(1,$binding);", ("$binding", canary));
        transaction.Commit();
      }
      VerifyBinding(connection);
      // A process restart or restored backup cannot turn an old unclaimed preparation into a new send.
      Execute(connection, null, "UPDATE intents SET state='uncertain' WHERE state='prepared';");
      ready = true;
    }
    catch (Exception error) when (Expected(error)) { Dispose(); }
  }

  internal bool IsAvailable { get { lock (keySync) return ready; } }

  internal JournalResult Prepare(JournalIdentity identity, string key, string digest, string action, string sdk, JsonElement prepared) => Run(connection =>
  {
    if (!Valid(identity) || !Hash(key) || !Hash(digest) || action is not ("refine" or "propose" or "comment" or "continue")
        || sdk is not { Length: > 0 and <= 64 } || !Version.TryParse(sdk, out _)) return new("unavailable");
    var scope = Scope(identity);
    using var transaction = connection.BeginTransaction(deferred: false);
    var previous = Select(connection, transaction, "key=$key", ("$key", key));
    if (previous is not null) return previous.Scope != scope ? new("denied") : previous.Digest != digest ? new("conflict") : Project(previous, "existing");
    previous = Select(connection, transaction, "scope=$scope AND terminal=0", ("$scope", scope));
    if (previous is not null) return Project(previous, "existing");
    using (var count = Command(connection, transaction, "SELECT COUNT(*) FROM intents"))
      if ((long)count.ExecuteScalar()! >= 10000) return new("unavailable");
    var encrypted = Protect(prepared);
    var id = Guid.NewGuid().ToString("N");
    Execute(connection, transaction, "INSERT INTO intents VALUES($id,$key,$digest,$scope,$action,$sdk,'prepared',$prepared,NULL,$created,0)",
        ("$id", id), ("$key", key), ("$digest", digest), ("$scope", scope), ("$action", action), ("$sdk", sdk),
        ("$prepared", encrypted), ("$created", DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture)));
    transaction.Commit();
    return new("prepared", id, prepared.Clone());
  });

  internal JournalResult Claim(JournalIdentity identity, string id, string key, string digest) => Run(connection =>
  {
    if (!Guid.TryParseExact(id, "N", out _) || !Valid(identity) || !Hash(key) || !Hash(digest)) return new("denied");
    using var transaction = connection.BeginTransaction(deferred: false);
    var row = Select(connection, transaction, "id=$id", ("$id", id));
    if (row is null || row.Scope != Scope(identity)) return new("denied");
    if (row.Key != key || row.Digest != digest) return new("conflict");
    if (row.State != "prepared" || row.Terminal) return Project(row, "existing");
    Execute(connection, transaction, "UPDATE intents SET state='claimed' WHERE id=$id AND state='prepared'", ("$id", id));
    transaction.Commit();
    return Project(row, "claimed");
  });

  internal JournalResult Read(JournalIdentity identity, string id) => Run(connection =>
  {
    if (!Valid(identity) || !Guid.TryParseExact(id, "N", out _)) return new("denied");
    var row = Select(connection, null, "id=$id", ("$id", id));
    return row is not null && row.Scope == Scope(identity) ? Project(row, row.State) : new("denied");
  });

  internal JournalResult Latest(JournalIdentity identity) => Run(connection =>
  {
    if (!Valid(identity)) return new("denied");
    var row = Select(connection, null, "scope=$scope ORDER BY rowid DESC", ("$scope", Scope(identity)));
    return row is null ? new("empty") : Project(row, row.State);
  });

  internal JournalResult Record(JournalIdentity identity, string id, JsonElement receipt, bool terminal) => Run(connection =>
  {
    if (!Valid(identity) || !Guid.TryParseExact(id, "N", out _)) return new("denied");
    using var transaction = connection.BeginTransaction(deferred: false);
    var row = Select(connection, transaction, "id=$id", ("$id", id));
    if (row is null || row.Scope != Scope(identity)) return new("denied");
    Execute(connection, transaction, "UPDATE intents SET receipt=$receipt,terminal=$terminal,state=CASE WHEN state='prepared' THEN 'uncertain' ELSE state END WHERE id=$id",
        ("$receipt", Protect(receipt)), ("$terminal", terminal || row.Terminal ? 1 : 0), ("$id", id));
    transaction.Commit();
    return new("recorded", id);
  });

  private JournalResult Run(Func<SqliteConnection, JournalResult> operation)
  {
    lock (keySync)
    {
      if (!ready) return new("unavailable");
      try
      {
        CheckFiles();
        using var connection = Connect();
        VerifyBinding(connection);
        return operation(connection);
      }
      catch (Exception error) when (Expected(error)) { return new("unavailable"); }
    }
  }
  private SqliteConnection Connect()
  {
    storage!.Verify(); keys!.Verify(); storage.SameFile(Database, database!);
    var connection = new SqliteConnection(new SqliteConnectionStringBuilder
    { DataSource = storage.Path + "/" + Database, Mode = SqliteOpenMode.ReadWrite, Cache = SqliteCacheMode.Private, Pooling = false, DefaultTimeout = 2 }.ToString());
    try
    {
      connection.Open();
      storage.SameFile(Database, database!);
      using (var size = Command(connection, null, "PRAGMA page_size"))
        if ((long)size.ExecuteScalar()! != 4096) throw new IOException("Unsupported journal page size.");
      Execute(connection, null, "PRAGMA trusted_schema=OFF; PRAGMA synchronous=FULL; PRAGMA max_page_count=12800;");
      return connection;
    }
    catch { connection.Dispose(); throw; }
  }
  private void VerifyBinding(SqliteConnection connection)
  {
    using var command = Command(connection, null, "SELECT version,binding FROM metadata");
    using var reader = command.ExecuteReader();
    if (!reader.Read() || reader.GetInt64(0) != 1 || Unprotect((byte[])reader[1]).GetProperty("journalId").GetString() != journalId || reader.Read())
      throw new IOException("Journal schema or protector binding is unavailable.");
  }
  private void CheckFiles()
  {
    storage!.Verify(); keys!.Verify();
    foreach (var path in Directory.EnumerateFileSystemEntries(storage.Path))
    {
      var name = System.IO.Path.GetFileName(path);
      if (name != Database && name != Database + "-wal" && name != Database + "-shm" && name != Database + "-journal") throw new IOException("Unexpected journal member.");
      using var file = storage.File(name);
    }
    lock (keySync)
    {
      var found = new HashSet<string>(StringComparer.Ordinal);
      foreach (var path in Directory.EnumerateFileSystemEntries(keys.Path))
      {
        var name = System.IO.Path.GetFileName(path);
        if (name != Marker && !(name.StartsWith("key-", StringComparison.Ordinal) && name.EndsWith(".xml", StringComparison.Ordinal))) throw new IOException("Unexpected journal key member.");
        using var file = keys.File(name); found.Add(name);
        if (name != Marker)
        {
          using var stream = new FileStream(file, FileAccess.Read);
          var buffer = new byte[65537];
          var count = stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);
          if (count == buffer.Length) throw new IOException("Journal key exceeds its bound.");
          var digest = Convert.ToHexStringLower(SHA256.HashData(buffer.AsSpan(0, count)));
          if (keyDigests.TryGetValue(name, out var previous) && previous != digest) throw new IOException("A journal key changed.");
          keyDigests[name] = digest;
        }
      }
      if (!keyNames.IsSubsetOf(found)) throw new IOException("Journal keys were removed.");
      keyNames.UnionWith(found);
    }
  }
  private byte[] Protect(JsonElement value)
  {
    var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
    if (bytes.Length > 65536) throw new IOException("Journal record exceeds its bound.");
    lock (keySync)
    {
      var result = protector!.Protect(bytes);
      if (result.Length > 65536) throw new IOException("Protected journal record exceeds its bound.");
      foreach (var path in Directory.EnumerateFiles(keys!.Path, "*.xml"))
      {
        var name = System.IO.Path.GetFileName(path);
        if (!keyNames.Contains(name)) keys.HardenNewKey(name);
      }
      keys.Sync(); CheckFiles();
      return result;
    }
  }
  private JsonElement Unprotect(byte[] bytes)
  {
    if (bytes.Length > 65536) throw new IOException("Journal record exceeds its bound.");
    using var value = JsonDocument.Parse(protector!.Unprotect(bytes)); return value.RootElement.Clone();
  }
  private JournalResult Project(Row row, string state) => new(state, row.Id, Unprotect(row.Prepared), row.Receipt is null ? null : Unprotect(row.Receipt));
  private sealed record Row(string Id, string Key, string Digest, string Scope, string State, byte[] Prepared, byte[]? Receipt, bool Terminal);
  private static Row? Select(SqliteConnection connection, SqliteTransaction? transaction, string predicate, params (string, object)[] parameters)
  {
    using var command = Command(connection, transaction, "SELECT id,key,digest,scope,state,prepared,receipt,terminal FROM intents WHERE " + predicate + " LIMIT 1", parameters);
    using var reader = command.ExecuteReader();
    return reader.Read() ? new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
        (byte[])reader[5], reader.IsDBNull(6) ? null : (byte[])reader[6], reader.GetBoolean(7)) : null;
  }
  private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? transaction, string sql, params (string, object)[] parameters)
  {
    var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = sql;
    foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
    return command;
  }
  private static void Execute(SqliteConnection connection, SqliteTransaction? transaction, string sql, params (string, object)[] parameters)
  { using var command = Command(connection, transaction, sql, parameters); command.ExecuteNonQuery(); }
  private static bool Valid(JournalIdentity value) => !string.IsNullOrEmpty(value.Owner) && value.Owner.Length <= 2048
      && value.ActorId is > 0 and <= 9_007_199_254_740_991 && value.RepositoryId is > 0 and <= 9_007_199_254_740_991 && value.IssueId is > 0 and <= 9_007_199_254_740_991;
  private static bool Hash(string value) => value is { Length: 64 } && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
  private static string Scope(JournalIdentity value) => Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value)));
  private static bool Expected(Exception error) => error is IOException or UnauthorizedAccessException or SqliteException or CryptographicException or JsonException or InvalidOperationException or ArgumentException;
  public void Dispose()
  {
    lock (keySync) { ready = false; database?.Dispose(); storage?.Dispose(); keys?.Dispose(); }
  }
}
