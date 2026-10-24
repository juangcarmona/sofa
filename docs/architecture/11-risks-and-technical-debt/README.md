---
title: Risks and technical debt
arc42-section: "11"
description: Known risks, technical debt, and their planned responses.
---

# 11. Risks and technical debt

## Risks

| Risk | Evidence | Response | First-slice posture |
| --- | --- | --- | --- |
| The documented SoFa handoff is not present in the current Lighthouse workflow | The router uses `workflow_dispatch`; stage workers use `workflow_call` and the self-hosted label. No SoFa process or wire contract exists in this repository. | Define and verify one explicit host adapter before claiming admission, leases, checkpoints, or supervision. | Blocking architectural gap. |
| The current gh-aw/Copilot projection is mistaken for the target engine path | The compiled Lighthouse worker runs gh-aw with `engine: copilot`; the accepted target runs persistent OpenCode on Bastion with the execution account's OpenAI Codex/ChatGPT authentication. No adapter is implemented. | Keep the current projection labelled transitional. Implement and verify the Lighthouse-job-to-SoFa handoff before invoking OpenCode; do not add GitHub API credentials for this path. | Blocking implementation gap. |
| Bastion Codex authentication is unavailable to the service account | OpenCode ChatGPT/Codex login is account-local, while the runner is a dedicated `github-runner` service account. | Authenticate and health-check OpenCode in the exact service context; keep credentials outside workspaces and never copy them into GitHub secrets or artifacts. | Blocking implementation and operations check. |
| Model selection becomes implicit in stage names | Lighthouse stages define lifecycle semantics, not model policy. | Define an observable OpenCode capability-to-model profile in Bastion configuration; do not encode model authorization as a new Lighthouse stage or gate. | Open implementation configuration. |
| The initial trigger is overstated as a generic GitHub event | The current SDLC router declares only `workflow_dispatch`; `workflow_call` is the worker composition boundary. | Treat manual dispatch as the initial event. Add issue/PR ingress or a central SoFa ingress only as a later, explicit adapter. | No webhook; no implicit event consumer. |
| The machine gate on `integrate` needs explicit semantic compatibility | The published `stages.json` declares `integrate.gate: done`, while the integration stage is described as an integration result and merge preparation rather than a new lifecycle gate. | Preserve Lighthouse's exact `stage-result` contract, treat `done` as the existing implementation gate rather than inventing another gate, and add a compatibility test before SoFa consumes it. | Must be resolved before stage-result handoff implementation. |
| Lighthouse and SoFa drift into two lifecycle engines | Lighthouse already owns route, stage, gate, and `stage-result` contracts. | Keep Lighthouse authoritative; SoFa stores references and operational outcomes only. | Must be prevented. |
| Stale worker publishes after recovery | A persistent host can outlive a worker process. | Lease expiry plus fencing token checked on every state-changing publication. | Must be tested. |
| Trusted isolation is insufficient for untrusted code | Containers share a kernel and repository code can be adversarial. | Admit only trusted repositories initially; reserve Docker Sandbox/microVM for a later decision. | Out of scope; reject or block untrusted input. |
| Central webhook becomes an accidental bypass | No current Lighthouse evidence requires one. | Use GitHub event -> Lighthouse router -> job -> SoFa runner. Add ingress only as a reviewed adapter. | No webhook. |
| Model gateway becomes a second engine authority | OpenCode/provider configuration, optional gateway policy, and Lighthouse lifecycle have different owners. | Keep provider concerns behind OpenCode/gateway; record bounded metadata only; keep SoFa out of model serving and Lighthouse out of model choice. | No model server in SoFa. |
| Single Bastion is a capacity bottleneck | The first topology has one persistent host and an initial target of five to six concurrent executions. | Make capacity and health observable; reject or queue work when resources are unhealthy; do not assume autoscaling. | Bounded concurrency on one host. |
| GitHub runner scope is mistaken for host scope | Personal repositories do not share a native account-level runner registration; repository-level registrations are separate even when installed on one machine. | Document one registration per participating repository and keep global admission/capacity in SoFa. | Must be explicit before Bastion setup. |
| Multiple registrations bypass the capacity rule | Several repository registrations on one host can receive jobs independently. | Require the SoFa runner adapter to admit work through one host-level capacity boundary before execution; allocate one isolated worker/workspace per admitted run. | Blocking for the multi-repository profile. |
| Contract version drift | SoFa selects Lighthouse package `0.9.0` and SDLC package `0.8.0`; the distributed SDLC metadata still declares contract version `0.5.0`. | Verify package assets and contract versions in implementation CI; pin the exact consumed catalog. | The package-versus-contract distinction is recorded; no compatibility guess. |
| Documentation is mistaken for implementation | Current source has no runtime. | Label target/proposed evidence and require runtime verification in the implementation phase. | No implementation claim. |

## Technical debt

- The logical Lighthouse-to-SoFa contract needs a versioned wire schema and a compatibility test once implementation is authorized.
- PostgreSQL placement, backup/restore, retention, and credential management remain operational decisions.
- The exact OpenCode process contract, cancellation semantics, tool allowlist, service-account authentication check, and workspace driver remain to be designed during implementation.
- The capability-to-model profile and the provider/gateway split for future providers remain operational configuration decisions; no model IDs are invented in architecture documentation.
- The authentication mechanism for the privileged local control boundary is not selected.
- The initial project/repository identity reconciliation and GitHub API rate-limit strategy are not implemented.
- Runtime health and gateway/model version reporting are target capabilities, not current evidence.
- The current architecture documents were empty ArcShape scaffolds; this change creates the first evidence-grounded target view, but it does not prove the target is installed.
