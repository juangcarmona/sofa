# Issue 16: supervised stage execution

The maintainer accepted ADR-0004 and authorized implementation and the presentations pilot on 2026-10-24. This plan implements that accepted design. The first operational milestone is triage/refine; integration still requires an explicit decision bound to the resulting PR head.

## Work and verification

- Consume the Lighthouse package's request, result and effect validators from a pinned installation outside the execution workspace. Test unknown fields, incompatible versions, source identity, unsupported scope and blocked results.
- Retain bounded structured validator diagnostics after rejection. The first live pilot exposed a generic process-exit error that hid the contract failure; reject inconsistent reports and omit process logs from operational errors.
- Supply verified refinement-approval evidence to the engine after checking the owner comment against the published predecessor and its publication time. A live refinement exposed the gap between host verification and the evidence included in engine context; retain the same check before publication.
- Implement a .NET 10 supervisor and PostgreSQL admission store. Use a repository-bound Unix socket, transactional global capacity one, immutable request identities and durable operational outcomes. Test contention, duplicate requests and changed payload rejection against PostgreSQL.
- Supervise one isolated OpenCode container with a read-only source snapshot, a writable result directory, a bounded deadline and no publication credential. Terminate the container before releasing capacity. Test cancellation and interrupted recovery; treat missing provider authentication as unavailable.
- Publish only Lighthouse-validated issue comments after verifying current source, issue content, policy and lease authority. Persist intent before dispatch and receipt afterwards. Reconcile by stable marker after uncertain publication; never blindly retry.
- Compose the store and supervisor with Aspire. Provide reproducible operator setup for the dedicated repository runner, engine credential volume and local socket. No public execution endpoint or changes to existing runner services.
- Connect an opted-in consumer workflow after the compatible Lighthouse package is released. Record actual GitHub issue, workflow, source, attempt and published refinement evidence. Fixtures prove contracts only.

## Completion gate

Run repository verification, .NET build/tests and PostgreSQL integration checks. The independent done-gate audit reconciles this plan against code and evidence. Report unresolved authentication, deployment, publication and end-to-end acceptance explicitly. The maintainer retains product, security, PR approval and delivery acceptance decisions.

## Implementation evidence

The first adapter, PostgreSQL coordination, kernel peer identity, container isolation, deadline/cancellation, session cleanup, workflow attestation and fenced issue-comment publisher are implemented and covered by .NET and Node tests in `npm run verify`. An independent audit reproduced and drove fixes for peer credentials, cancellation races, stale receipt upload and missing-session cleanup.

A real OpenAI request from the engine returned `SOFA_PROVIDER_OK`. A real OpenCode triage produced a Lighthouse-valid blocked report from a deliberately incomplete source fixture; the .NET adapter stopped the container and removed its session. Repeated cleanup against the actual image also passed. These are runtime integration checks, not GitHub end-to-end acceptance.

The dedicated `presentations` runner is registered separately from the existing SoFa runner. The consumer now adopts Lighthouse CLI 0.9.1 and SDLC 0.9.0. After correcting a workflow expression context, [the first live triage](https://github.com/juangcarmona/presentations/actions/runs/40117524911) reached execution but failed result validation before publication. Its operational artifact records that failure. After fixing stage evidence scope, [triage passed and published its receipt](https://github.com/juangcarmona/presentations/actions/runs/40118570862). The owner approved refinement against that exact result. [The first refinement](https://github.com/juangcarmona/presentations/actions/runs/40119808297) published a blocked report because the engine lacked the approval evidence already verified by the host. The context fix passes all 35 runtime tests and the independent implementation audit; successful live refinement remains pending. The first product PR is beyond this triage/refine implementation; the roadmap retains implementation/integration adapters and their human gates.
