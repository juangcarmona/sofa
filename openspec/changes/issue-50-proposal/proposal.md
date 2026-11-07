# Read one complete issue workspace

<!-- pdac-scope: cited -->

## Why

The operations overview cannot explain an ordinary issue, a long conversation or a blocked proposal. The signed-in owner needs that context inside SoFa before deciding what to request.

## What Changes

- Add an authenticated issue view and read API, reached by repository/number or an observed issue.
- Read the complete issue body and conversation through explicit bounded pages, with source IDs, times and incomplete/unavailable states.
- Present source-backed lifecycle reports, dispatches, reservations, related PRs/checks and bounded artifact summaries separately from GitHub workflow status. Preserve association and revision evidence; never select a run by recency.
- Retain stale context after temporary errors and erase it after authority loss or account changes.

## Capabilities

### New Capabilities

- `issue-context`: paginated source context and independently identified lifecycle evidence.

### Modified Capabilities

None. This implements the read/query part of accepted CHG-AUTHENTICATED-OPERATIONS-001. Existing owner admission and GitHub user authority remain #49.

## Impact

The existing .NET Workspace source adapter and browser assets gain an issue reader. No workflow, execution controller, source mutation, credential fallback or durable command journal is introduced. Commands stay #47/#48; inbox and boards reuse this view under #51/#52. The PR is stacked on #84 until that dependency passes its own gate.

## Product impact and drift

The unchanged full product baseline reviewed for #49 remains authoritative. Direct dependencies and the expanded FR-ISSUE-CONTEXT graph identify the artifacts cited below. TERM-WORK-UNIT, TERM-WORK-COMMAND, TERM-EXECUTION-RUN and TERM-DELIVERY-EVIDENCE distinguish displayed facts; ACT-FACTORY-OWNER and BC-BACKLOG-TRIAGE provide context. Execution, scheduling and production identity neighbours are inspected and unchanged. No product-definition drift is proposed.

Issue: <https://github.com/juangcarmona/sofa/issues/50>.

## Product grounding

<!-- pdac:cite id="UC-OPERATE-ISSUE" digest="sha256:e1c11763cbb6a3f3ff8aa2a7101bc672a6a45595a256134fb04d1d2070f66d2f" -->

<!-- pdac:cite id="FR-ISSUE-CONTEXT" digest="sha256:11f4c196c15675fa20c05bb320f02a2bab4bdbf96cf275a408c849924faed270" -->

<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

<!-- pdac:cite id="BR-EXPLICIT-WORK-COMMANDS" digest="sha256:5d2f308b670bdf6e577d2205ebc246ec56505253d81d6a5a5efe0b92b55d735e" -->

<!-- pdac:cite id="BR-LAN-PRIVILEGE-BOUNDARY" digest="sha256:318c7d8d4d3b2e27109e24628f99dd1825494fa9858dc13410032d73650f1cc8" -->

<!-- pdac:cite id="QR-MOBILE-OPERATIONS" digest="sha256:2485b5877f0872af3dbdaa0a7ac00ac7651b28173adf6e3cf8d9ed643029bb35" -->

<!-- pdac:cite id="BR-EXPLAINABLE-SIGNALS" digest="sha256:1b8ec288714357b03b030a921f70b7f1dd0e82a590d05430cc8b4bf638c933a9" -->
