---
title: Risks and technical debt
arc42-section: "11"
description: Known risks, technical debt, and their planned responses.
---

# 11. Remaining limitations

The current observer reads only the latest twenty runs and the first hundred open issue records per repository. It is not a complete audit and does not verify artifact contents. The read-only web UI and periodic browser refresh are implemented; authenticated controls remain future work. The installed router admits the owner and supports Refine, Propose, Implement, Integrate and Review entry under stage-specific evidence and gate checks. Runner loss may leave a reservation requiring operator reconciliation. Legacy execution code remains opt-in for rollback and must not regain product authority.

<!-- pdac:cite id="FR-DELIVERY-EVIDENCE" digest="sha256:5a56ea8e0030730dfdf0d8fd51027c45c2e6ddd65393b6d7726c85c27e543074" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed delivery risks and owners

| Risk | Owning work |
| --- | --- |
| Source dispatch receipt received versus response lost | Command journal plus Lighthouse correlation/deduplication contract; no time-based matching |
| Ordinary comments may resume needs-info or invalidate task digest | Lighthouse explicit intent contract; UI pre-submit effect explanation |
| Installed contract differs from source/latest documentation | Pin and test the adopted package; incompatibility is explicit |
| OAuth/App user permission differs from router actor | Real user-token dispatch/comment test before claiming authorization |
| Cached capabilities become stale before admission | Server refresh and source admission recheck; no optimistic approval |
| Incomplete collector hides issues or evidence | Paginated query adapter with coverage and unavailable states |
| Health data is partial or not comparable | Inspectable source definitions and no invented values/scores |
| Development host exposes real operational credentials | Test credentials locally; protected production boundary per ADR-0007 |
| Proposed storage lost or corrupted | Backup/recovery; source reconciliation before any resend |

Issue #47's refined request now has a blocked proposal report identifying identity/deployment and exact correlation. Owner decisions address identity direction; shared/source receipt behavior still needs implementation evidence. Legacy issue #16 describes the superseded supervisor and must not be treated as a dependency of this operational backlog. Its history and closure decision are preserved.
