---
name: local-admission-and-fenced-publication
description: Admit repository jobs through a local supervisor and fence every privileged effect.
status: Accepted
date: 2026-10-24
deciders: [Juan García Carmona]
tags: [runtime, isolation, recovery, publication]
---

# ADR-0004: Admit jobs locally and fence privileged publication

## Context

Accepted ADRs assign repository delivery to Lighthouse and runtime operation to SoFa on one Bastion. Repository-scoped runners are required for the personal account, but runner labels do not invoke SoFa or enforce capacity. PostgreSQL fencing protects database writes only; an old worker holding a GitHub token could still push after losing its lease.

The user selected `presentations`, locally adopting Lighthouse 0.9.0, for the first end-to-end pilot. OpenCode remains the baseline engine. The user permits direct Codex as an alternative, but no alternate engine is needed to settle the interface or build two adapters now.

## Decision

We will use a local runner bridge that submits one Lighthouse stage request to the independently supervised .NET process over a Unix-domain socket. The socket protocol is versioned; its payload uses the Lighthouse-owned request contract. Start with submit, status, cancel and result retrieval. The bridge waits for a terminal outcome while the worker runs under supervisor ownership.

Bind bridge access to a registered repository identity with OS permissions and separate service identities for participating repositories. Existing shared `github-runner` deployment is not evidence of that isolation. The supervisor checks the configured repository, trusted workflow revision, pinned contract, requested capabilities, source revision, deadline and policy before admission. Reject arbitrary shell commands and caller-selected filesystem paths. A compromised trusted workflow remains inside the host trust boundary and must not receive supervisor administration privileges.

Use PostgreSQL transactions for admission, one active execution per work item, a global pilot capacity of one and the existing default of one active execution per project. The initial capacity is a rollout configuration, not a change to the accepted five-to-six-run sizing target. Allocate one isolated workspace and process group per attempt.

Keep GitHub mutation credentials in a trusted publication component, outside the engine. Load enforcement code and effective policy from a pinned host-owned installation outside the agent-writable workspace. All issue, branch, PR and merge effects pass through Lighthouse validation and a SoFa fencing check. Serialize the final lease check, effect dispatch and recovery handover for the same publication resource. Record an effect identity, target revision and receipt. If a network failure leaves publication uncertain, reconcile the remote object before permitting retry or a new owner to publish. A database lease cannot make a remote API call transactional.

Keep OpenCode installed and authenticated persistently, but create a separate execution process/session per run. Persist checkpoints at verified operation boundaries, not model turns. A restart recovers from verified artifacts and remote receipts; it does not presume that a model session can safely resume an interrupted tool call.

## Consequences

- Bastion needs no public callback or network-exposed execution API for the first pilot.
- One shared supervisor enforces capacity across repository registrations while Lighthouse remains the lifecycle authority.
- Stale workers cannot publish directly because they have no publication credential. Uncertain remote effects can block recovery until reconciled.
- Distinct OS identities, workspace isolation and credential injection require deployment work; multiple directories under one user are not equivalent isolation.
- OpenCode and a possible later Codex adapter use the same stage boundary, but changing the baseline engine still requires reconciliation of the product constraint and ADR-0003. No generic plugin system is introduced.

## Alternatives considered

- Execute OpenCode directly in the runner: insufficient for shared admission, durable recovery and fenced publication.
- Add a central webhook and public API first: adds another ingress and authentication surface without helping the selected GitHub Actions transport.
- Give every engine a GitHub write token and rely on lease checks in its prompt: stale processes can bypass the database and write remotely.
- One shared OpenCode conversation for the factory: mixes workspaces, cancellation and authority across runs.
- Build OpenCode and Codex adapters together: deferred until the first adapter fails a concrete requirement or a second consumer needs another engine.

## References

- [ADR-0001](0001-lighthouse-delivery-workflow-sofa-bastion-runtime.md)
- [ADR-0002](0002-bounded-parallel-execution-on-bastion.md)
- [ADR-0003](0003-bastion-opencode-codex-authentication.md)
- [Runtime interactions](../06-runtime-view/README.md)
- [Pilot acceptance](../../roadmap/first-pilot.md)
- [Recoverable execution](../../product/model/requirements/functional/fr-recoverable-execution.md)
