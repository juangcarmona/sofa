---
title: Building block view
arc42-section: "05"
description: The static decomposition of the system into building blocks.
---

# 5. Building block view

## Level 1: system

SoFa is one runtime boundary inside a larger delivery system. The following ownership map is authoritative for this architecture:

| Building block | Owner | Responsibility | Explicitly does not own |
| --- | --- | --- | --- |
| Lighthouse router | Lighthouse | Validate event inputs, route, prerequisites, and composition mode. | SoFa leases, workspace lifecycle, or model execution. |
| Lighthouse stage worker | Lighthouse | Execute exactly one configured stage and produce the current `stage-result` contract. | Later stages, merge authorization, or runtime recovery. |
| GitHub Actions job | GitHub / consumer repository | Carry event and workflow inputs, select runner capability, upload artifacts, and report job status. | Product approval or durable SoFa operational state. |
| SoFa scheduler and admission | SoFa | Select eligible work, enforce capacity/policy, and admit explicit execution requests up to healthy capacity. | Lighthouse route or gate semantics. |
| SoFa supervisor and Worker | SoFa | Acquire leases, create checkpoints, supervise one worker per run, cancel, timeout, recover, and publish only while fenced. | Lifecycle stage meaning or human decisions. |
| SoFa runner registration set | SoFa, deployed on Bastion | Provide one repository-scoped GitHub runner registration per participating personal repository; a future adapter hands an accepted job contract to the shared supervisor. | The registrations are not an account-wide GitHub runner, do not create multiple SoFa workers, and do not own lifecycle semantics. |
| Execution workspace / sandbox | SoFa runtime | Isolate checkout, tool state, and temporary artifacts for one run; clean or quarantine it at termination. | Persistent source of truth. |
| OpenCode adapter and process | SoFa integration plus OpenCode (target) | Start and supervise the persistent Bastion OpenCode process with scoped inputs, Codex/ChatGPT account authentication, cancellation, and bounded operational observation. | It is not invoked by the current Lighthouse gh-aw/Copilot projection; it does not own model serving, route selection, gates, or merge decisions. |
| Operational store | SoFa | Persist coordination, recovery, lease, checkpoint, health, and evidence references. | Credentials, raw source archives, full prompts, or full transcripts by default. |
| Model gateway | Bastion infrastructure | Route model calls, mask sensitive data, meter tokens, enforce security, and expose model health. | Agent lifecycle or GitHub delivery authority. |
| Model services | Bastion/provider | Perform inference. | SoFa or Lighthouse state. |

### Observed Lighthouse worker boundary

The current generated Lighthouse worker is not a thin callback to SoFa. Its source declares `engine: copilot` and `runs-on: [self-hosted, linux, lighthouse-sdlc]`; the compiled workflow installs and invokes the Copilot CLI in the agent job, then validates and uploads the Lighthouse `stage-result`. SoFa has no application or runner implementation in this repository that intercepts that job. The target decomposition therefore distinguishes this transitional GitHub Actions projection from the future SoFa execution worker, which invokes persistent OpenCode on Bastion.

## Level 2: building blocks

```mermaid
flowchart TB
    accTitle: SoFa and Lighthouse building blocks
    subgraph Lighthouse["Lighthouse in the consumer repository"]
        Router[Router]
        Stage[Stage workers]
        Contract[Stage-result contract]
        Router --> Stage
        Stage --> Contract
    end

    subgraph Actions["GitHub Actions"]
        Event[Event / dispatch]
        Job[Reusable stage job]
        Artifact[Stage-result artifact]
        Event --> Job
        Job --> Artifact
    end

    subgraph Sofa["SoFa on Bastion"]
        Admission[Admission and scheduling]
        Supervisor[Supervisor]
        Runner[Runner registrations per repository]
        State[Operational state]
        Admission --> Supervisor
        Supervisor --> Runner
        Supervisor --> State
    end

    subgraph Execution["Per-run execution"]
        Workspace[Ephemeral workspace / sandbox]
        Worker[Bounded execution workers]
        OpenCode[OpenCode + Codex auth]
        Workspace --> Worker
        Worker --> OpenCode
    end

    Router --> Job
    Job --> Runner
    Runner --> Admission
    Supervisor --> Workspace
    OpenCode --> Providers[OpenAI Codex / future providers]
    Providers --> Models[Model services]
    Job --> Artifact
```

### Proposed future Lighthouse-to-SoFa handoff

This is a logical target contract, not a contract currently published by Lighthouse or implemented by SoFa. The current transport is a GitHub Actions job assigned to the SoFa runner capability; it is not a central webhook and does not by itself call the SoFa runtime. The exact wire schema belongs to the implementation phase and must be versioned without redefining Lighthouse's `stage-result` schema.

The request from the Lighthouse job to SoFa must identify:

| Field | Meaning | Authority |
| --- | --- | --- |
| `contractVersion` | Version of the Lighthouse-to-SoFa handoff contract. | Joint interface, versioned explicitly. |
| `executionRunId` | Stable identifier for this attempt and its evidence. | Lighthouse supplies or correlates it; SoFa persists it. |
| `projectRef` and `repositoryRef` | Consumer project and repository identity. | GitHub / Lighthouse identity. |
| `workItemRef` | Issue or work-item identity being executed. | GitHub / Lighthouse identity. |
| `route` | Selected Lighthouse route, such as `supervised` or `plan-only`. | Lighthouse. |
| `stage` | One of the six current Lighthouse stages. | Lighthouse. |
| `sourceRevision` | Commit or checkout identity from which the job starts. | GitHub / job. |
| `prerequisiteRefs` | Prior stage-result or gate evidence needed by the route. | Lighthouse. |
| `capability` | Required runner capability, initially `[self-hosted, linux, lighthouse-sdlc]`. | Lighthouse / GitHub Actions. |
| `deadline` and `cancellation` | Bounded execution and cancellation intent. | Joint contract; SoFa enforces the operational bound. |
| `workspacePolicy` | Trusted-repository classification and allowed isolation profile. | SoFa policy, constrained by the request. |

SoFa returns an operational result containing `executionRunId`, admission status, run status, lease and checkpoint references, `workspaceRef`, `commitRef` when observed, cancellation or timeout outcome, recovery result, bounded failure reason, and references to Lighthouse stage result and delivery evidence. It does not return a replacement stage result. The exact Lighthouse `stage-result` is produced and validated by Lighthouse and remains its authority.

Minimum admission outcomes are `accepted`, `rejected`, `blocked`, or `cancelled`. A successful SoFa handoff means that execution was admitted or completed operationally; it never means that a Lighthouse gate, PR approval, merge, release, or production decision was accepted.
