using Npgsql;
using System.Text.Json.Nodes;

namespace SoFa.Runtime;

public sealed record Run(string Id, string Repository, long Issue, string Request, string State, long Fence, DateTime Deadline, string? Result, string? Intent, string? Receipt, string? Error);

public sealed class RunStore(NpgsqlDataSource database)
{
    const int AdmissionLock = 7416001;
    public async Task Initialize(CancellationToken token)
    {
        await using var cmd = database.CreateCommand("""
            CREATE TABLE IF NOT EXISTS sofa_runs (
              id text PRIMARY KEY, repository text NOT NULL, issue bigint NOT NULL,
              request text NOT NULL, digest text NOT NULL, state text NOT NULL,
              fence bigint NOT NULL DEFAULT 1, deadline timestamptz NOT NULL,
              result text, intent text, receipt text, error text, created_at timestamptz NOT NULL DEFAULT now());
            ALTER TABLE sofa_runs ADD COLUMN IF NOT EXISTS work_claimed boolean NOT NULL DEFAULT false;
            """);
        await cmd.ExecuteNonQueryAsync(token);
    }
    static Run Read(NpgsqlDataReader r) => new(r.GetString(0), r.GetString(1), r.GetInt64(2), r.GetString(3), r.GetString(4), r.GetInt64(5), r.GetDateTime(6), r.IsDBNull(7) ? null : r.GetString(7), r.IsDBNull(8) ? null : r.GetString(8), r.IsDBNull(9) ? null : r.GetString(9), r.IsDBNull(10) ? null : r.GetString(10));
    const string Columns = "id, repository, issue, request, state, fence, deadline, result, intent, receipt, error";
    public async Task<Run?> ForWorkflow(long workflow, long issue, CancellationToken token)
    {
        await using var cmd = database.CreateCommand($"SELECT {Columns} FROM sofa_runs WHERE request::jsonb->>'workflowRunId'=$1 AND issue=$2");
        cmd.Parameters.AddWithValue(workflow.ToString(System.Globalization.CultureInfo.InvariantCulture)); cmd.Parameters.AddWithValue(issue);
        await using var reader = await cmd.ExecuteReaderAsync(token);
        return await reader.ReadAsync(token) ? Read(reader) : null;
    }
    public async Task<Run?> Get(string id, CancellationToken token)
    {
        await using var cmd = database.CreateCommand($"SELECT {Columns} FROM sofa_runs WHERE id=$1");
        cmd.Parameters.AddWithValue(id);
        await using var reader = await cmd.ExecuteReaderAsync(token);
        return await reader.ReadAsync(token) ? Read(reader) : null;
    }
    public async Task<Run> Admit(string raw, CancellationToken token)
    {
        var input = JsonNode.Parse(raw)!.AsObject();
        var id = input["runId"]!.GetValue<string>();
        await using var connection = await database.OpenConnectionAsync(token);
        await using var tx = await connection.BeginTransactionAsync(token);
        await using var locked = new NpgsqlCommand("SELECT pg_advisory_xact_lock($1)", connection, tx);
        locked.Parameters.AddWithValue(AdmissionLock);
        await locked.ExecuteNonQueryAsync(token);
        await using var existing = new NpgsqlCommand($"SELECT {Columns} FROM sofa_runs WHERE id=$1", connection, tx);
        existing.Parameters.AddWithValue(id);
        await using (var reader = await existing.ExecuteReaderAsync(token))
        {
            if (await reader.ReadAsync(token))
            {
                var run = Read(reader);
                if (run.Request != raw) throw new Rejected("Idempotency identity was reused with a different request");
                return run;
            }
        }
        await using var capacity = new NpgsqlCommand("SELECT count(*) FROM sofa_runs WHERE work_claimed OR state IN ('admitted','running','publishing','publication-uncertain','cancelling')", connection, tx);
        if (Convert.ToInt64(await capacity.ExecuteScalarAsync(token)) != 0) throw new Rejected("Pilot capacity is occupied; retry the same request after the active attempt finishes");
        await using var attempts = new NpgsqlCommand("SELECT count(*) FROM sofa_runs WHERE repository=$1 AND issue=$2 AND request::jsonb->>'stage'=$3 AND request::jsonb->>'sourceRevision'=$4", connection, tx);
        attempts.Parameters.AddWithValue(input["repository"]!.GetValue<string>());
        attempts.Parameters.AddWithValue(input["issue"]!.GetValue<long>());
        attempts.Parameters.AddWithValue(input["stage"]!.GetValue<string>());
        attempts.Parameters.AddWithValue(input["sourceRevision"]!.GetValue<string>());
        if (Convert.ToInt64(await attempts.ExecuteScalarAsync(token)) >= 3) throw new Rejected("Pilot attempt limit reached for this issue, stage and source revision");
        await using var insert = new NpgsqlCommand("INSERT INTO sofa_runs(id,repository,issue,request,digest,state,deadline) VALUES($1,$2,$3,$4,$5,'admitted',$6)", connection, tx);
        insert.Parameters.AddWithValue(id);
        insert.Parameters.AddWithValue(input["repository"]!.GetValue<string>());
        insert.Parameters.AddWithValue(input["issue"]!.GetValue<long>());
        insert.Parameters.AddWithValue(raw);
        insert.Parameters.AddWithValue(Lighthouse.Hash(raw));
        insert.Parameters.AddWithValue(DateTime.Parse(input["deadline"]!.GetValue<string>()).ToUniversalTime());
        await insert.ExecuteNonQueryAsync(token);
        await tx.CommitAsync(token);
        return (await Get(id, token))!;
    }
    public async Task<Run?> Claim(CancellationToken token)
    {
        await using var cmd = database.CreateCommand($"UPDATE sofa_runs SET state='running' WHERE id=(SELECT id FROM sofa_runs WHERE state='admitted' ORDER BY created_at LIMIT 1 FOR UPDATE SKIP LOCKED) AND state='admitted' RETURNING {Columns}");
        await using var reader = await cmd.ExecuteReaderAsync(token);
        return await reader.ReadAsync(token) ? Read(reader) : null;
    }
    public async Task<bool> Transition(Run run, string expected, string state, CancellationToken token, string? result = null, string? intent = null, string? receipt = null, string? error = null)
    {
        await using var cmd = database.CreateCommand("UPDATE sofa_runs SET state=$1,result=COALESCE($2,result),intent=COALESCE($3,intent),receipt=COALESCE($4,receipt),error=$5 WHERE id=$6 AND fence=$7 AND state=$8");
        cmd.Parameters.AddWithValue(state);
        foreach (var value in new[] { result, intent, receipt, error }) cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text, Value = (object?)value ?? DBNull.Value });
        cmd.Parameters.AddWithValue(run.Id); cmd.Parameters.AddWithValue(run.Fence); cmd.Parameters.AddWithValue(expected);
        return await cmd.ExecuteNonQueryAsync(token) == 1;
    }
    public async Task<Run?> Cancel(string id, CancellationToken token)
    {
        await using var connection = await database.OpenConnectionAsync(token);
        await using var tx = await connection.BeginTransactionAsync(token);
        await using var check = new NpgsqlCommand($"SELECT {Columns} FROM sofa_runs WHERE id=$1 FOR UPDATE", connection, tx);
        check.Parameters.AddWithValue(id);
        Run run;
        await using (var reader = await check.ExecuteReaderAsync(token))
        {
            if (!await reader.ReadAsync(token)) return null;
            run = Read(reader);
        }
        if (run.State is "publishing" or "publication-uncertain") throw new Rejected("Publication must be reconciled before cancellation");
        if (run.State is "admitted" or "running")
        {
            await using var update = new NpgsqlCommand("UPDATE sofa_runs SET state=$1 WHERE id=$2", connection, tx);
            update.Parameters.AddWithValue(run.State == "admitted" ? "cancelled" : "cancelling");
            update.Parameters.AddWithValue(id);
            await update.ExecuteNonQueryAsync(token);
        }
        await tx.CommitAsync(token);
        return await Get(id, token);
    }
    public async Task<List<Run>> Unfinished(CancellationToken token)
    {
        await using var cmd = database.CreateCommand($"SELECT {Columns} FROM sofa_runs WHERE work_claimed OR state IN ('running','publishing','publication-uncertain','cancelling')");
        await using var reader = await cmd.ExecuteReaderAsync(token);
        var runs = new List<Run>();
        while (await reader.ReadAsync(token)) runs.Add(Read(reader));
        return runs;
    }
    public async Task MarkWork(Run run, bool claimed, CancellationToken token)
    {
        await using var cmd = database.CreateCommand("UPDATE sofa_runs SET work_claimed=$1 WHERE id=$2 AND fence=$3");
        cmd.Parameters.AddWithValue(claimed); cmd.Parameters.AddWithValue(run.Id); cmd.Parameters.AddWithValue(run.Fence);
        if (await cmd.ExecuteNonQueryAsync(token) != 1) throw new Rejected("Work reservation fence changed");
    }
    public async Task<bool> Interrupt(Run run, CancellationToken token)
    {
        await using var cmd = database.CreateCommand("UPDATE sofa_runs SET state='interrupted',fence=fence+1,error='Supervisor restarted; execution stopped. A fresh authorized request is required.' WHERE id=$1 AND fence=$2 AND state IN ('running','cancelling')");
        cmd.Parameters.AddWithValue(run.Id); cmd.Parameters.AddWithValue(run.Fence);
        return await cmd.ExecuteNonQueryAsync(token) == 1;
    }
    public async Task<string> PublishFenced(Run run, Func<Task<string>> publish, CancellationToken token)
    {
        await using var connection = await database.OpenConnectionAsync(token);
        await using var tx = await connection.BeginTransactionAsync(token);
        await using var check = new NpgsqlCommand("SELECT fence,state,deadline FROM sofa_runs WHERE id=$1 FOR UPDATE", connection, tx);
        check.Parameters.AddWithValue(run.Id);
        await using (var reader = await check.ExecuteReaderAsync(token))
        {
            if (!await reader.ReadAsync(token) || reader.GetInt64(0) != run.Fence || reader.GetString(1) != "publishing" || reader.GetDateTime(2) <= DateTime.UtcNow)
                throw new Rejected("Publication authority is stale or expired");
        }
        var receipt = await publish();
        await using var update = new NpgsqlCommand("UPDATE sofa_runs SET state='waiting-human',receipt=$1 WHERE id=$2", connection, tx);
        update.Parameters.AddWithValue(receipt); update.Parameters.AddWithValue(run.Id);
        await update.ExecuteNonQueryAsync(token);
        await tx.CommitAsync(token);
        return receipt;
    }
}
