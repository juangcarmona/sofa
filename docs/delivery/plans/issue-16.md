# Issue 16: supervised stage execution

The maintainer accepted ADR-0004 and authorized implementation and the presentations pilot on 2026-10-24. This plan implements that accepted design. The first operational milestone is triage/refine; integration still requires an explicit decision bound to the resulting PR head.

## Work and verification

- Consume the Lighthouse package's request, result and effect validators from a pinned installation outside the execution workspace. Test unknown fields, incompatible versions, source identity, unsupported scope and blocked results.
- Implement a .NET 10 supervisor and PostgreSQL admission store. Use a repository-bound Unix socket, transactional global capacity one, immutable request identities and durable operational outcomes. Test contention, duplicate requests and changed payload rejection against PostgreSQL.
- Supervise one isolated OpenCode container with a read-only source snapshot, a writable result directory, a bounded deadline and no publication credential. Terminate the container before releasing capacity. Test cancellation and interrupted recovery; treat missing provider authentication as unavailable.
- Publish only Lighthouse-validated issue comments after verifying current source, issue content, policy and lease authority. Persist intent before dispatch and receipt afterwards. Reconcile by stable marker after uncertain publication; never blindly retry.
- Compose the store and supervisor with Aspire. Provide reproducible operator setup for the dedicated repository runner, engine credential volume and local socket. No public execution endpoint or changes to existing runner services.
- Connect an opted-in consumer workflow after the compatible Lighthouse package is released. Record actual GitHub issue, workflow, source, attempt and published refinement evidence. Fixtures prove contracts only.

## Completion gate

Run repository verification, .NET build/tests and PostgreSQL integration checks. The independent done-gate audit reconciles this plan against code and evidence. Report unresolved authentication, deployment, publication and end-to-end acceptance explicitly. The maintainer retains product, security, PR approval and delivery acceptance decisions.
