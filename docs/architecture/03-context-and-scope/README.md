---
title: System scope and context
arc42-section: "03"
description: The system boundary, its users, and its external interfaces.
---

# 3. System scope and context

## Business context

SoFa is the runtime side of a personal Software Factory. GitHub remains the external backlog, source, pull-request, check, and delivery-evidence authority. Lighthouse runs the repository lifecycle in the consumer repository. SoFa supplies bounded execution capacity and operational recovery on Bastion.

```mermaid
flowchart LR
    accTitle: SoFa system context and external boundaries
    Operator["Human operator\nproduct, architecture, integration"]
    Repo["Consumer repository\nLighthouse / gh-aw configuration"]
    GitHub["GitHub\nevents, issues, PRs, checks, artifacts"]
    Actions["GitHub Actions\nworkflow_dispatch, workflow_call, job"]

    subgraph Bastion["Persistent Bastion"]
        SoFa["SoFa Runtime / Supervisor"]
        Runner["SoFa runner capability\nself-hosted GitHub runner"]
        Sandbox["Ephemeral workspace / sandbox"]
        Engine["OpenCode execution engine"]
        Provider["OpenAI Codex / future providers"]
        Models["Local or provider model services"]
        Store["Operational persistence"]
    end

    Operator -. decisions .-> GitHub
    Repo --> Actions
    GitHub --> Actions
    Actions --> Runner
    Runner --> SoFa
    SoFa --> Sandbox
    Sandbox --> Engine
    Engine --> Provider
    Provider --> Models
    SoFa --> Store
    Actions --> GitHub
    SoFa --> GitHub
```

The current Lighthouse SDLC router is triggered by `workflow_dispatch`. Its stage workers are invoked through `workflow_call`; the repository guardrail workflow has separate `pull_request`, `push`, and `workflow_dispatch` triggers and is not an SDLC execution trigger. There is no current issue or pull-request event that invokes the SDLC router, and there is no central SoFa webhook.

The current workflow projection places the agent job on `[self-hosted, linux, lighthouse-sdlc]`. That label is a GitHub Actions scheduling capability, not a callback into a SoFa process. The published workflow currently selects gh-aw/Copilot and therefore does not implement the target OpenCode path; the observed run failed at Copilot credential activation before reaching the agent job. The target architecture requires an explicit host adapter before SoFa can receive an execution contract or supervise a separate OpenCode process. A later ingress can be introduced only as an additional transport adapter, not as a new lifecycle authority.

### Personal-repository runner topology

The factory owner has personal GitHub repositories rather than an organization. The physical deployment is shared, but GitHub's native runner registrations remain repository-scoped:

```mermaid
flowchart LR
    accTitle: Personal repository runner registrations on one Bastion
    subgraph Personal["Juan's personal GitHub account"]
        RepoA["Consumer repository A\nLighthouse workflows"]
        RepoB["Consumer repository B\nLighthouse workflows"]
        RepoN["Consumer repository N\nLighthouse workflows"]
    end

    subgraph Actions["GitHub Actions"]
        JobA["Job for repository A"]
        JobB["Job for repository B"]
        JobN["Job for repository N"]
    end

    subgraph Bastion["One persistent Bastion"]
        RunnerA["Runner registration A"]
        RunnerB["Runner registration B"]
        RunnerN["Runner registration N"]
        Admission["SoFa admission / capacity"]
        Workers["Bounded SoFa execution workers\ninitial capacity target: 5-6"]
        RunnerA --> Admission
        RunnerB --> Admission
        RunnerN --> Admission
        Admission --> Workers
    end

    RepoA --> JobA --> RunnerA
    RepoB --> JobB --> RunnerB
    RepoN --> JobN --> RunnerN
```

This is one host with several repository-scoped GitHub registrations, not one account-wide GitHub runner. Each registration needs its own directory, service, token, and workspace root. SoFa must serialize admitted execution across those registrations; the labels alone cannot do that. The current implementation has neither these registrations nor the SoFa admission adapter, so the diagram is a target deployment profile, not an implementation claim.

## Technical context

| External system or boundary | Interface in the initial architecture | SoFa interpretation |
| --- | --- | --- |
| Consumer repository | Lighthouse configuration, generated gh-aw workflows, repository checkout, artifacts. | Source of the execution request and stage-result location; not SoFa-owned lifecycle state. |
| GitHub Actions | Event trigger, router job, `workflow_call`, runner labels, job status, artifacts. | Initial dispatch and execution transport. |
| GitHub API | Issues, PRs, checks, commits, workflow runs, review and merge evidence. | External source/evidence authority; SoFa stores references and observations. |
| Lighthouse router/workers | Route/stage input, prerequisites, stage prompts, stage-result contract. | Lifecycle authority to be invoked, never reimplemented by SoFa. |
| SoFa runner capability | Current GitHub Actions capability label supplied by the Bastion runner; future local bridge from a job to the SoFa runtime. | The label alone does not create admission, supervision, or a result handoff. |
| OpenCode | Scoped execution request, Codex/ChatGPT account authentication, and bounded process/tool results. | External engine supervised by SoFa; not a lifecycle or persistence authority. |
| OpenAI Codex and future providers | Model inference selected by OpenCode/provider configuration. | Lighthouse stages, SoFa lifecycle, or merge authority. |
| Model gateway | Optional provider routing, masking, token metadata, and health when enabled. | Existing infrastructure proof; SoFa observes and correlates it. |
| Local/provider model services | Model inference. | Provider/runtime dependency; no product or merge authority. |

### Current versus target execution boundary

The current Lighthouse package is the observed execution authority at the GitHub Actions job boundary: the worker workflow selects the gh-aw `copilot` engine, runs on the self-hosted capability, and validates/uploads the `stage-result`. This is transitional configuration, not the target engine path. The SoFa runtime, SoFa runner adapter, OpenCode process boundary, Bastion OpenAI/Codex authentication, and provider gateway remain target components with no implementation evidence in this repository.

The target adds a SoFa-owned handoff inside the job. The job carries the Lighthouse execution contract to the shared SoFa admission boundary; SoFa then creates the isolated workspace and invokes the persistent Bastion OpenCode installation. The job does not select a GitHub-hosted AI engine or carry an OpenAI API secret.

### Scope boundaries

In scope are runtime coordination, scheduling, admission, leases, fencing, checkpoints, recovery, capacity, worker supervision, workspace lifecycle, isolation, operational persistence, health, and Bastion operation.

Out of scope are Lighthouse stage semantics, route policy, gate decisions, agent prompts, repository lifecycle definitions, model serving, provider policy, and human acceptance. SoFa may record references to these things, but it does not become their source of truth.
