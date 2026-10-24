---
title: Runtime view
arc42-section: "06"
description: Runtime behavior and important interactions between building blocks.
---

# 6. Runtime view

## Runtime scenarios

### Current Lighthouse execution boundary

```mermaid
sequenceDiagram
    participant D as GitHub workflow_dispatch
    participant R as Lighthouse router
    participant V as Route validation job
    participant W as Lighthouse workflow_call worker
    participant A as GitHub Actions self-hosted job
    participant C as gh-aw Copilot engine
    participant L as Lighthouse stage-result validator
    participant G as GitHub artifacts/checks

    D->>R: route, stage, composition, input
    R->>V: Validate route and prerequisites
    V->>W: workflow_call for one stage or ff
    W->>A: Schedule on [self-hosted, linux, lighthouse-sdlc]
    A->>C: Execute compiled gh-aw agent job
    C->>L: Write stage-result.json
    L->>G: Validate and upload stage result
```

This is the current observed path. The self-hosted runner executes the Lighthouse job; no SoFa process, OpenCode adapter, lease, checkpoint, or SoFa operational result is present in the path. The `lighthouse.yml` repository guardrail workflow is a separate verification workflow and is not the SDLC router.

### Target supervised execution after an explicit SoFa adapter exists

```mermaid
sequenceDiagram
    participant GH as GitHub workflow_dispatch
    participant R as Lighthouse router
    participant W as Lighthouse stage worker
    participant A as GitHub Actions job
    participant SR as SoFa runner
    participant SS as SoFa supervisor
    participant DB as Operational store
    participant X as Ephemeral worker/workspace
    participant O as OpenCode
    participant G as Model gateway

    GH->>R: Manual dispatch
    R->>R: Validate route, stage, prerequisites, composition
    R->>W: Invoke one stage or explicit ff chain
    W->>A: Start reusable stage job
    A->>SR: Select [self-hosted, linux, lighthouse-sdlc]
    SR->>SS: Future explicit host-adapter handoff
    SS->>DB: Create run and acquire lease
    DB-->>SS: Fencing token and lease expiry
    SS->>X: Create isolated workspace and worker
    X->>O: Execute scoped OpenCode work if that adapter is selected
    O->>G: Request routed model service
    G-->>O: Model response and bounded metadata
    X->>DB: Checkpoint sequence and heartbeat
    X-->>A: Stage result and bounded evidence handoff
    A->>A: Validate and upload Lighthouse stage-result artifact
    A-->>R: Job result
    SS->>DB: Record outcome, evidence references, and release lease
```

The router remains responsible for choosing a route and stage. The Lighthouse worker remains responsible for stage semantics and writing the current `stage-result` artifact. GitHub Actions remains responsible for job orchestration and artifact publication. The GitHub runner executes the job assigned to its capability. Only after an explicit adapter exists can SoFa admit and supervise a separate execution boundary, create the ephemeral workspace, record operational state, and enforce cancellation, timeout, fencing, and recovery. OpenCode performs agentic work behind the gateway only on the accepted engine path; it does not choose the lifecycle transition.

`ff` is a Lighthouse composition mode. Its next stage is invoked only after the previous job succeeds and its declared result is valid. A failed, blocked, unavailable, or missing prerequisite stops the chain. SoFa reports the operational outcome; it does not turn a successful job into an implicit human decision.

### Failure, lease expiry, and recovery

```mermaid
sequenceDiagram
    participant S as SoFa supervisor
    participant DB as Operational store
    participant Old as Old worker
    participant WS as Workspace
    participant New as Recovery worker
    participant H as Human decision
    participant L as Lighthouse job

    S->>DB: Observe missed heartbeat or timeout
    S->>DB: Mark run interrupted/suspected lost
    S->>WS: Quarantine or verify workspace identity
    DB-->>S: Latest valid checkpoint and lease expiry
    S->>DB: Expire old lease and issue new fencing token
    Old-->>DB: Late checkpoint/result publication
    DB-->>Old: Reject stale fencing token
    alt Policy and evidence permit recovery
        S->>New: Start with latest checkpoint
        New->>WS: Verify source, workspace, and commit identity
        New->>DB: Heartbeat and monotonic checkpoint
        New-->>L: Complete or report bounded failure
    else Recovery is unsafe or exhausted
        S->>DB: Record recovery-required outcome
        S->>H: Request human decision
    end
```

Recovery never treats a missing heartbeat as success. It is permitted only when the latest checkpoint, workspace identity, source revision, and policy make continuation safe. A stale worker cannot publish because every state-changing publication is checked against the current fencing token. If recovery bounds are exhausted or evidence is uncertain, the run stops for a human decision.

### Cancellation and timeout

Cancellation is an explicit operational outcome. The supervisor stops admitting new work for the run, requests engine cancellation, records the last acknowledged checkpoint, releases or expires the lease, and cleans or quarantines the workspace. A timeout follows the same path but records the configured timeout reason. Neither outcome is converted into a Lighthouse gate or delivery acceptance.
