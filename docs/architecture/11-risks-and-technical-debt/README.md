---
title: Risks and technical debt
arc42-section: "11"
description: Known risks, technical debt, and their planned responses.
---

# 11. Risks and technical debt

## Risks

| Risk | Evidence | Response | First-slice posture |
| --- | --- | --- | --- |
| Lighthouse and SoFa drift into two lifecycle engines | Lighthouse already owns route, stage, gate, and `stage-result` contracts. | Keep Lighthouse authoritative; SoFa stores references and operational outcomes only. | Must be prevented. |
| Stale worker publishes after recovery | A persistent host can outlive a worker process. | Lease expiry plus fencing token checked on every state-changing publication. | Must be tested. |
| Trusted isolation is insufficient for untrusted code | Containers share a kernel and repository code can be adversarial. | Admit only trusted repositories initially; reserve Docker Sandbox/microVM for a later decision. | Out of scope; reject or block untrusted input. |
| Central webhook becomes an accidental bypass | No current Lighthouse evidence requires one. | Use GitHub event -> Lighthouse router -> job -> SoFa runner. Add ingress only as a reviewed adapter. | No webhook. |
| Model gateway becomes a second engine authority | OpenCode and model services are distinct from lifecycle control. | Keep gateway/provider concerns behind the engine boundary; record bounded metadata only. | No model server in SoFa. |
| Single Bastion is a capacity bottleneck | The first topology has one persistent host and one active worker. | Make capacity and health observable; do not assume autoscaling. | One active execution worker. |
| Contract version drift | SoFa selects Lighthouse package `0.9.0` and SDLC package `0.8.0`; the distributed SDLC metadata still declares contract version `0.5.0`. | Verify package assets and contract versions in implementation CI; pin the exact consumed catalog. | The package-versus-contract distinction is recorded; no compatibility guess. |
| Documentation is mistaken for implementation | Current source has no runtime. | Label target/proposed evidence and require runtime verification in the implementation phase. | No implementation claim. |

## Technical debt

- The logical Lighthouse-to-SoFa contract needs a versioned wire schema and a compatibility test once implementation is authorized.
- PostgreSQL placement, backup/restore, retention, and credential management remain operational decisions.
- The exact OpenCode process contract, cancellation semantics, tool allowlist, and workspace driver remain to be designed during implementation.
- The authentication mechanism for the privileged local control boundary is not selected.
- The initial project/repository identity reconciliation and GitHub API rate-limit strategy are not implemented.
- Runtime health and gateway/model version reporting are target capabilities, not current evidence.
- The current architecture documents were empty ArcShape scaffolds; this change creates the first evidence-grounded target view, but it does not prove the target is installed.
