---
title: Cross-cutting concepts
arc42-section: "08"
description: Concepts and rules that affect multiple building blocks.
---

# 8. Cross-cutting concepts

## Domain model

The following is the minimum operational vocabulary. It is not a replacement for ProductShape domain definitions and it is not a database schema.

| Concept | Operational meaning | Must not be confused with |
| --- | --- | --- |
| Project / repository | Stable consumer-project and GitHub repository identity. | A local workspace or a single run. |
| Work item | A referenced unit of work, normally tied to a GitHub issue or equivalent Lighthouse input. | A stage result or an execution attempt. |
| Route | Lighthouse route selected for the run. | A SoFa scheduling policy. |
| Stage | One current Lighthouse lifecycle stage. | `ready`, `planned`, or `done`, which are gates. |
| Execution run | One operational attempt to execute one work item/stage under a policy. | Successful delivery or human acceptance. |
| Lease | Time-bounded authority for one SoFa worker to act on a run. | A GitHub Actions job or a Lighthouse gate. |
| Fencing token | Monotonic authority value checked on publication so a stale worker cannot write. | A retry count. |
| Checkpoint sequence | Monotonic run-local sequence naming the latest resumable boundary. | A full transcript or arbitrary process dump. |
| Workspace reference | Identity and location reference for an isolated execution workspace. | Persistent source code storage. |
| Commit reference | Observed source, branch, or resulting commit identity. | A claim that a commit was merged. |
| Cancellation / timeout | Explicit operational termination reason. | A failed or accepted Lighthouse gate. |
| Outcome | Bounded operational result such as completed, failed, blocked, cancelled, timed out, or recovery-required. | A human decision. |
| Evidence reference | Pointer to a GitHub artifact, check, PR, commit, stage result, or bounded local observation. | Telemetry or a product requirement. |
| Worker heartbeat | Liveness observation for a SoFa worker. | Proof that the work succeeded. |
| Recovery result | Record of resumed, stopped, rejected, or human-required recovery. | A fresh lifecycle stage. |
| Runner host | The persistent Bastion machine that can host several repository-scoped GitHub runner registrations. | A GitHub runner registration or a SoFa execution worker. |
| Runner registration | One GitHub Actions registration tied to one personal repository and installed in one directory/service on Bastion. | An account-wide runner, a lease, or a capacity decision. |

### Persistence categories

| Category | SoFa stores | SoFa does not store by default |
| --- | --- | --- |
| Operational state | IDs, statuses, leases, fencing, checkpoints, timestamps, bounded reasons, references, health observations. | Product decisions or a duplicate lifecycle state machine. |
| Lighthouse stage result | A reference, digest, status observation, and artifact location. | A second or rewritten `stage-result` contract. |
| Delivery evidence | GitHub commit/PR/check/artifact/workflow references, observed result, timestamp, and limitations. | An inferred merge, release, deployment, or production acceptance. |
| Telemetry | Correlated traces, metrics, logs, token counts, durations, and bounded dimensions. | Secrets, raw prompts, source code, or full transcripts. |
| Human decision | Explicit actor, decision, timestamp, scope, and evidence reference when the workflow exposes it. | An implicit approval inferred from CI or agent success. |

The operational store must support at least `execution_run`, `work_item`, project/repository reference, route, stage, lease, fencing token, checkpoint sequence, workspace reference, commit reference, cancellation, timeout, outcome, evidence reference, worker heartbeat, and recovery result. The exact relational schema is implementation work.

Runner registrations are deployment identity, not operational execution state. SoFa records the selected repository and registration identity as bounded run metadata, but it does not persist registration tokens or credentials. A registration may receive a GitHub job; only SoFa admission may make that job an active execution under the global capacity policy.

<!-- pdac:cite id="FR-RECOVERABLE-EXECUTION" digest="sha256:d5bed96280fd88240c3492cc4ff6a609ad80e6d6b2891e536c36f36a78772678" -->

## Engine and gateway boundary

The current Lighthouse projection uses gh-aw with the Copilot engine. gh-aw provides its own workflow-level sandbox, credential isolation, agent firewall, threat detection, Safe Outputs, token steering, and OpenTelemetry hooks in the compiled job. Those are observed mechanisms of the current transitional Lighthouse projection and are not SoFa-owned state.

The target Bastion hosts persistent OpenCode under the dedicated execution account. OpenCode uses the account's OpenAI Codex/ChatGPT authentication and selects models through its provider configuration. SoFa supervises the process boundary and correlates bounded operational metadata; it does not duplicate gh-aw controls, become a model server, or move the credential into GitHub. A gateway remains an optional provider-routing boundary for providers and controls that require it, and its presence must be proved by Bastion evidence.

## Security and privacy

The existing LAN surface is not a privileged control plane. Unauthenticated LAN access must not dispatch execution, acquire a privileged lease, launch OpenCode, mutate GitHub, approve a gate, or expose secrets. Privileged control remains loopback-only and separately authenticated until a new accepted security decision changes it.

The trusted-repository first slice uses least privilege, scoped credentials, an ephemeral workspace, no host Docker socket, and no unnecessary container privileges. Credentials are injected only for the duration and scope of the operation and are removed before cleanup. Forks and untrusted code need a stronger isolation boundary, such as Docker Sandbox or a microVM, before they are admitted; that decision is intentionally outside the first slice.

SoFa supervises OpenCode but does not become a model server. OpenCode owns the initial provider/model selection through its Bastion configuration; an enabled gateway owns routing, masking, token metering, and provider-facing security controls for the providers it serves. SoFa correlates bounded operational metadata and health with the run. It must not duplicate provider policy or persist full model conversations by default.

## Observability and operations

Health is operational evidence, not delivery evidence. The minimum health surface covers process version, worker heartbeat age, current lease and expiry, checkpoint lag, eligible-work/capacity counts, PostgreSQL connectivity, OpenCode process outcome, gateway/model-service health, verification failures, recovery-required count, and version drift. A healthy process is not proof that a stage passed or that a merge is authorized.

Every operational publication is correlated by `executionRunId`, worker identity, lease ID, fencing token, and checkpoint sequence where relevant. Dimensions remain bounded by project, work item class, route, stage, outcome, and worker. Masking excludes credentials, authorization headers, prompts, source text, and high-cardinality content.

### Authority and gates

| Lighthouse point | Preserved behavior | SoFa behavior |
| --- | --- | --- |
| `triage` | Stops at `triaged`. | Schedules only the declared stage and records the result reference. |
| `refine` | Stops at `ready`. | Does not treat readiness as an execution grant outside the route. |
| `propose` | Stops at `planned`. | Does not apply or approve the proposal. |
| `implement` | Stops at `done`. | Does not invoke later stages unless Lighthouse explicitly composes them. |
| `integrate` | Prepares integration/merge according to Lighthouse policy. | Does not infer merge authorization from success or mergeability. |
| `review` | Closes the review without a new gate. | Records the review evidence and does not add `deploy`. |

`ff` is the explicit composition mechanism. It stops when a required stage fails or is blocked. Evidence and acceptance remain separate: deterministic checks, agent output, stage results, GitHub delivery evidence, and human decisions are independently recorded.

## Lifecycle storage and one writer

GitHub remains authoritative for source, issues, PRs and checks. Lighthouse owns the transition rules and validates delivery events. SoFa may persist those validated events and cache a rebuildable projection; it owns only the operational state machine for admission/execution/recovery. Labels are a provider projection, not authorization. Existing Agentic SDLC `status:*` instructions need an explicit adoption migration before Lighthouse becomes the consumer's sole label writer.

Persist run/attempt identities, source and policy digests, leases, checkpoints, event references and effect receipts in PostgreSQL. Keep large verification artifacts in bounded artifact storage with hashes and retention policy; a reference to an expiring GitHub artifact is insufficient long-term evidence. An effect ledger records planned, confirmed or uncertain remote outcomes so recovery can reconcile them.

The database can enforce operational uniqueness and monotonic checkpoints. It cannot guarantee atomicity with GitHub, and an engine session ID cannot prove a tool call's remote effect. Missing metrics remain unavailable; report measured coverage for usage totals instead of treating unreported calls as free.
