# Factory implementation backlog

Historical runtime plan. [ADR-0006](../architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md) supersedes SoFa execution ownership; consult the current architecture before using this plan.

These are proposed delivery slices for the [roadmap](software-factory.md), not claims of created issues or completed implementation. The existing architecture work remains linked to issue #14. Plan future implementation in separate work items so its validation and scope remain clear.

| Slice | Owner | Dependency | Acceptance |
| --- | --- | --- | --- |
| P0: consumer readiness | `presentations` and Lighthouse | Lighthouse L1 | Preserve and commit current adoption/application work; establish a meaningful `verify` command; reconcile authorities, labels and workflow package resolution; record the exact revision and repository runner. |
| S1: execution bridge and durable admission | SoFa | Lighthouse L2 | Compose .NET 10, Aspire and PostgreSQL; one registered local client submits a scoped request; duplicate requests return the same execution; unauthorized identity/version/scope is rejected; global limit one is enforced transactionally. |
| S2: bounded engine execution | SoFa | S1 | Authenticate OpenCode in the execution service context; run a read-only stage in an isolated workspace; enforce deadline/cancel; keep provider credentials and publication credentials outside the checkout. |
| P1: automatic triage/refine | Lighthouse and consumer | P0, S2, Lighthouse L3 | A real issue, optional clarification and owner gate produce a visible refined story, one effect receipt and correlated artifacts. No source change or PR is needed for this milestone. |
| S3: recovery and fenced publisher | SoFa | S2 | Persist checkpoints and effect intents; kill a worker; reject stale publications; reconcile a remote timeout before retry; verify cancellation, cleanup/quarantine and bounded recovery. |
| P2: implementation through PR | Lighthouse and consumer | P1, S3, Lighthouse L4 | Approved proposal produces one bounded patch, real verification and one PR through the trusted publisher. Independent review is recorded separately. |
| P3: integration evidence | Consumer and SoFa | P2 | Explicit merge authority is bound to the current head; verify the resulting default-branch commit and checks; retain evidence beyond GitHub artifact expiry according to policy. |
| S4: multi-repository scheduling | SoFa | P3 | A second repository has its own registration/identity; global/per-project limits hold under contention, revocation works and measured resources justify any concurrency increase. |

## Delivery invariants

Each slice names the policy and source revision it used, and tests behavior at a trust or recovery boundary. Fixtures do not count as external-client execution. Successful model output does not count as published refinement. An open PR does not count as accepted delivery. Preserve package and provider ownership during every upgrade.

Database and bridge work can use a fake engine until S2; label it a transport/contract test. Real `presentations` acceptance cannot use the fake engine or bypass Lighthouse gates. Keep model selection, service-account login and any later engine substitution visible as operational dependencies.
