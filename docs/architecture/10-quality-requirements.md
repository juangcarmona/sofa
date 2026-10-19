---
title: Quality requirements
arc42-section: "10"
description: Quality goals and concrete scenarios for evaluating them.
---

# 10. Quality requirements

**Approved architecture.** Juan accepted this architecture on 2026-10-19; issue 6 implements the initial slice. Product intent is recorded in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Quality goals

The proposed design prioritizes evidence accuracy, partial-result usefulness, replaceable adapters, and observable collection. Product acceptance remains in [CHG-INITIAL](../product/changes/completed/chg-initial/change.md).

## Evaluation scenarios

Implementation checks live in [SoFa.Tests](../../tests/SoFa.Tests/SoFa.Tests.csproj); execution evidence is recorded with [issue 6](../delivery/ff/6/20261019-attention/summary.md).

| Scenario | Expected architectural evidence |
| --- | --- |
| Juan’s own open non-draft PR has no review request | One approval finding; drafts and closed PRs excluded; another reviewer’s approval does not remove it |
| Old revision fails or latest retry succeeds | Pure classification tests exclude superseded failures |
| GitHub denies access or fails midway through pagination | Adapter tests and page show incomplete coverage without an all-clear |
| One repository fails while four succeed | Orchestration returns four useful results and an explicit failure |
| Two refresh requests overlap | Single collection; snapshots never expose half-built state |
| Application restarts | No snapshot is presented as persisted or current |
| A refresh completes | Aspire shows correlated outcomes and durations; credentials are absent |
| UI or GitHub transport changes | Attention tests run without web host or network dependencies |

Keep tests under `tests/`, grouped by Attention, GitHub adapter, and Web concerns. Use source fixtures for deterministic semantics and a small authorized runtime smoke check during implementation.
