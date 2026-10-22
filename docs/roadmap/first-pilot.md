# First recoverable engineering execution pilot

This is a future acceptance scenario. It is not executable in S0 because the current Product Change is draft and no execution runtime is installed.

## Scenario

Use one small, low-risk GitHub issue from any configured project whose acceptance criteria do not require privileged access, production credentials, destructive commands, or an unresolved product decision. The issue must be linked to the future Product Change or an accepted implementation work item before the pilot begins.

## Acceptance flow

1. **Triage:** GitHub issue is imported with repository and issue identity, type, project, priority, risk, acceptance criteria, and source link. No duplicate local work item is created on repeated refresh.
2. **Shape:** The issue is split into one small work unit with explicit scope, dependencies, verification steps, and out-of-scope items. Its dependency graph is eligible and its autonomy profile is recorded.
3. **Authorize:** The configured policy evaluates the work unit. Product and architecture review may proceed through deterministic and semantic mechanisms; security, privileged-operation, and destructive-data impact require a human gate.
4. **Execute:** The independent Worker acquires one lease, starts an execution run, invokes OpenCode in the isolated workspace, and records setup and implementation checkpoints.
5. **Recover:** Stop the Worker after a checkpoint. A new Worker observes the expired/closed lease, reacquires it with a new fencing token, validates workspace identity, and resumes from the latest checkpoint without duplicating the completed step.
6. **Verify:** Deterministic local checks run and are recorded. The run stops on a failure or missing evidence; a green result does not approve a PR or product decision.
7. **PR and evidence:** A PR is opened with the issue link, changed files, checks, limitations, and evidence references. CI and review state are captured from GitHub.
8. **Authorized integration:** SoFa obtains the integration authorization required by the configured policy. A critical-impact change reaches a human gate; the Worker cannot infer authorization from a green check or a PR being mergeable.
9. **Delivery verification:** After integration, SoFa identifies the exact commit on the default branch, verifies the relevant default-branch pipelines and generated artifacts, and records source identities, timestamps, results, and any deployment limitation. Production deployment is not required for this first pilot. It reports unknown or unavailable delivery evidence explicitly.

## Pass criteria

- Every transition is durable and linked to the same work item and run.
- At most one valid lease exists for the work unit; the stale Worker cannot publish after fencing.
- Recovery resumes at a checkpoint or stops with a human-action reason; it never silently restarts or claims success.
- No unauthenticated LAN request can launch OpenCode, mutate GitHub, acquire a privileged lease, or approve integration.
- The final report distinguishes deterministic checks, human decisions, source evidence, and unknowns.
- The final delivery evidence names the exact default-branch commit, relevant pipeline runs, generated artifacts, timestamps, and any evidence that was unavailable or stale.
