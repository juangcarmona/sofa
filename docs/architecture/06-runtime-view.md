---
title: Runtime view
arc42-section: "06"
description: Runtime behavior and important interactions between building blocks.
---

# 6. Runtime view

**Approved architecture.** Juan accepted this architecture on 2026-10-19; issue 6 implements the initial slice. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Refresh and review

Realizes the accepted [refresh](../product/model/use-cases/uc-refresh-attention.md) and [review](../product/model/use-cases/uc-review-attention.md) use cases.

<!-- pdac:cite id="UC-REFRESH-ATTENTION" digest="sha256:17763f5433dfc0af4ab461c5a93c786b5755d6953aa9c51cbc31f3da57550abd" -->

<!-- pdac:cite id="UC-REVIEW-ATTENTION" digest="sha256:b4782cc66c2d2f6c4dc86c9600248248fc46b910882b8a74729cd426f3c290a1" -->

1. The web host validates a local refresh request, then calls Attention. Concurrent refresh requests join or report the existing refresh rather than starting duplicate collection.
2. Attention reads the configured five-repository allowlist and asks the GitHub adapter for evidence. Each repository/category returns observations plus collection status.
3. The adapter traverses relevant pages, uses bounded timeouts/retries, and preserves source identifiers, revisions, conclusions, timestamps, and URLs. Authentication, rate-limit, and pagination failures become explicit coverage outcomes.
4. Attention applies classification and deduplication rules, including Juan’s authored non-draft PRs. For PR checks it checks the head again; a changed head invalidates the affected observations and triggers a bounded reread or an explicit incomplete result.
5. Attention atomically replaces the latest snapshot, including partial outcomes. The host renders it with the observation interval and evidence links. Reading the page alone does not refresh or mutate GitHub.

## Failure behavior

An unavailable category cannot suppress another repository’s successful findings. An incomplete refresh never inherits old findings as if freshly collected. A process-wide interruption leaves the last completed snapshot visibly dated and reports the failed refresh separately. On restart, the empty state says no collection has occurred.

GitHub is not a transactional snapshot source. Show collection start/end times and make revision mismatches explicit. No claim of simultaneous cross-repository truth is made.
