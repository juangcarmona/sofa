# First recoverable engineering execution pilot

This is a future acceptance scenario. The target architecture is now documented, but no execution runtime is installed, so this scenario remains non-executable until the implementation phase is separately authorized.

The pilot uses Lighthouse's existing supervised route and its current stage semantics. It does not add a SoFa lifecycle stage: `triage` produces `triaged`, `refine` produces `ready`, `propose` produces `planned`, `implement` produces `done`, `integrate` prepares integration/merge evidence, and `review` closes without a new gate. `ff` is explicit Lighthouse composition and stops on failure or missing evidence.

## Recommended first vertical slice

The implementation should begin with one trusted consumer repository and one already-authorized `implement` job on the existing Lighthouse supervised route. Triage, refinement, proposal, route validation, stage prompts, stage-result validation, and artifact upload remain Lighthouse responsibilities and are consumed as existing contracts. The SoFa slice adds only the runner handoff, admission, one lease, one execution run, one isolated workspace, one supervised worker, checkpoint/heartbeat persistence, cancellation/timeout handling, fencing, and one recovery path.

The pilot repository must have its own repository-scoped GitHub Actions runner registration on Bastion. A later personal-factory expansion repeats that registration for each participating repository on the same host; it does not create an account-wide runner registration. The shared SoFa admission boundary remains responsible for enforcing the configured concurrency across those registrations, with an initial sizing target of five to six isolated executions.

The slice explicitly excludes multi-project scheduling policy, unattended execution, hotfix/stacked route variations, integration/merge execution, review execution, release or production deployment, untrusted forks, a central webhook, autoscaling, and multiple Bastion hosts. It must leave the repository and Lighthouse contract unchanged apart from the documented runner capability integration.

### Vertical-slice acceptance criteria

- A Lighthouse job reaches the SoFa runner through `[self-hosted, linux, lighthouse-sdlc]` without a central SoFa webhook.
- SoFa accepts only a versioned request containing the project, repository, work item, route, stage, source revision, and deadline/cancellation context.
- Exactly one active lease and one active SoFa execution worker exist for the run.
- The workspace is isolated, has no host Docker socket, receives only scoped credentials, and is cleaned or quarantined at termination.
- The worker records a heartbeat and at least one monotonic checkpoint before completion.
- A forced worker stop leaves a durable recovery result; the recovery worker receives a new fencing token and resumes only from the latest valid checkpoint.
- A stale worker's late checkpoint or result is rejected and cannot change the run outcome.
- The Lighthouse stage result remains the exact Lighthouse contract and is uploaded by the Lighthouse job; SoFa stores a reference rather than rewriting it.
- Cancellation, timeout, failure, blocked state, and successful stage completion remain distinguishable operational outcomes.
- The acceptance report separates SoFa operational state, Lighthouse stage result, delivery evidence, telemetry, and human decisions.
- No merge, release, production, or new lifecycle gate is inferred from a successful execution or CI result.

## Scenario

Use one small, low-risk GitHub issue from any configured project whose acceptance criteria do not require privileged access, production credentials, destructive commands, or an unresolved product decision. The issue must be linked to the future Product Change or an accepted implementation work item before the pilot begins.

## Acceptance flow

1. **Triage:** GitHub issue is imported with repository and issue identity, type, project, priority, risk, acceptance criteria, and source link. No duplicate local work item is created on repeated refresh. Lighthouse produces the `triaged` result.
2. **Shape:** The issue is split into one small work unit with explicit scope, dependencies, verification steps, and out-of-scope items. Its dependency graph is eligible and its autonomy profile is recorded. Lighthouse refinement produces the `ready` result.
3. **Authorize:** The configured policy evaluates the work unit. Product and architecture review may proceed through deterministic and semantic mechanisms; security, privileged-operation, and destructive-data impact require a human gate. Lighthouse proposal produces the `planned` result before implementation.
4. **Execute:** The independent SoFa Worker acquires one lease, starts an execution run, invokes persistent OpenCode on Bastion with the dedicated execution account's OpenAI Codex/ChatGPT authentication in the isolated workspace, and records setup and implementation checkpoints. Lighthouse implementation produces the `done` result; it does not invoke later stages implicitly.
5. **Recover:** Stop the Worker after a checkpoint. A new Worker observes the expired/closed lease, reacquires it with a new fencing token, validates workspace identity, and resumes from the latest checkpoint without duplicating the completed step.
6. **Verify:** Deterministic local checks run and are recorded. The run stops on a failure or missing evidence; a green result does not approve a PR or product decision.
7. **PR and evidence:** A PR is opened with the issue link, changed files, checks, limitations, and evidence references. CI and review state are captured from GitHub.
8. **Authorized integration:** Lighthouse integration prepares the integration/merge result. SoFa obtains the integration authorization required by the configured policy. A critical-impact change reaches a human gate; the Worker cannot infer authorization from a green check or a PR being mergeable.
9. **Delivery verification:** After integration, SoFa identifies the exact commit on the default branch, verifies the relevant default-branch pipelines and generated artifacts, and records source identities, timestamps, results, and any deployment limitation. Production deployment is not required for this first pilot. It reports unknown or unavailable delivery evidence explicitly.

## Pass criteria

- Every transition is durable and linked to the same work item and run.
- At most one valid lease exists for the work unit; the stale Worker cannot publish after fencing.
- Recovery resumes at a checkpoint or stops with a human-action reason; it never silently restarts or claims success.
- No unauthenticated LAN request can launch OpenCode, mutate GitHub, acquire a privileged lease, or approve integration.
- The final report distinguishes deterministic checks, human decisions, source evidence, and unknowns.
- The final delivery evidence names the exact default-branch commit, relevant pipeline runs, generated artifacts, timestamps, and any evidence that was unavailable or stale.

The first pilot deliberately excludes a central SoFa webhook, autoscaling, multiple Bastion hosts, untrusted fork execution, Docker Sandbox/microVM enforcement, production deployment, and any new Lighthouse lifecycle stage.
