# S0-to-pilot implementation backlog

These are proposed issue slices, not created GitHub issues. They are ordered by dependency and intentionally small. Each issue must go through the shared lifecycle and GitHub `status:*` labels; no issue is authorized by this document.

| Order | Proposed issue | Outcome | Depends on |
| --- | --- | --- | --- |
| 1 | `S0-01` Remove the obsolete Attention implementation | Application source, tests, delivery evidence, and accepted Attention ADR are removed from the implementation surface; the historical ProductShape change remains inert. | None |
| 2 | `S0-02` Reconcile Lighthouse and provider ownership | Clean isolated-worktree adoption plan with exact versions, conflicts, and owner-approved apply set. | S0-01 |
| 3 | `S0-03` Accept the initial runtime/security architecture | **Completed by issue #11:** ADR and approved architecture update covering .NET 10/Aspire, PostgreSQL, Worker, OpenCode, Lighthouse, Bastion, and control-boundary security. | S0-02 |
| 4 | `S1-01` Add the GitHub backlog projection | Read issues/projects and reconcile a local projection without replacing GitHub authority. | S0-03, Lighthouse adoption |
| 5 | `S1-02` Shape one dependency-aware work unit | Persist priority, risk, acceptance criteria, dependency edges, and an explicit autonomy profile for one issue. | S1-01 |
| 6 | `S1-03` Persist execution runs and leases | PostgreSQL migrations and transactional lease/fencing behavior with tests for expiry and contention. | S1-02 |
| 7 | `S1-04` Add checkpointed Worker/OpenCode adapter | One bounded low-risk capability, cancellation, sanitized telemetry, and no privileged command path. | S1-03 |
| 8 | `S1-05` Recover an interrupted run | Resume from the last valid checkpoint or stop explicitly; prove stale Worker fencing. | S1-04 |
| 9 | `S1-06` Record verification and PR evidence | Capture checks, PR, review state, and source revisions without inferring human approval. | S1-05 |
| 10 | `S1-07` Gate authorized integration and delivery verification | Explicit human integration gate followed by post-integration artifact/commit verification. | S1-06 |
| 11 | `S1-08` Run the first pilot | Execute [the future pilot](first-pilot.md) and retain durable evidence and limitations. | S1-07 |
| 12 | `S2-01` Add multi-project scheduling and maintenance | Only after the pilot: dependency-aware selection, WIP/risk limits, health signals, and stale-run maintenance. | S1-08 |

## Acceptance criteria by slice

### S0-01 — remove obsolete implementation

- No application runtime or Attention source remains under `src/`.
- The old .NET/Aspire setup, Attention tests, and Attention delivery evidence are removed.
- ProductShape's applied Software Factory model remains unchanged.
- Repository Attention remains only as inert historical ProductShape material.

### S0-02 — Lighthouse reconciliation

- Current dirty paths, hooks, agents, skills, CI, ProductShape, ArcShape, package versions, and provider owners are inventoried.
- Published versions are verified and no stale local version is silently treated as current.
- Lighthouse dry-run output classifies missing paths, conflicts, drift, and proposed writes.
- No provider-owned or dirty file is overwritten.
- The Lighthouse-owned hooks replace the previous hooks; the old hooks are preserved outside the repository, and CI-only checks remain accounted for.

### S0-03 — runtime/security architecture (completed by issue #11)

- Durable state includes work units, dependencies, runs, leases/fencing, checkpoints, evidence, and worker liveness.
- Any existing unauthenticated LAN surface remains non-privileged and cannot reach execution, GitHub writes, privileged commands, or integration gates.
- The existing unauthenticated LAN surface cannot reach execution, GitHub writes, privileged commands, or integration gates.
- PostgreSQL/OpenCode installation remains deferred until the implementation phase is authorized.

### S1-01 through S1-07 — pilot foundation

- Each slice has tests and source evidence for its acceptance criteria.
- Every external write and human decision is explicit and attributable.
- Failure, cancellation, restart, stale lease, missing evidence, and unavailable source states are visible.
- CI, PR, integration, and delivery evidence are linked to the work item and run.
