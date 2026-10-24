---
title: Solution strategy
arc42-section: "04"
description: Fundamental decisions and strategies that shape the architecture.
---

# 4. Solution strategy

## Technology decisions

The implementation target is intentionally conservative: a modular .NET 10 SoFa process composed by Aspire, PostgreSQL for durable operational state, a separately supervised Worker, and persistent OpenCode on Bastion. This is a target shape, not current implementation evidence. The first slice must not require a broker, a fleet, KEDA, autoscaling, a microservice decomposition, or an ephemeral host.

The execution engine and provider boundary are separate concerns. Lighthouse provides task and stage semantics. The published Lighthouse worker projection currently runs gh-aw with the `copilot` engine on the selected self-hosted runner and owns stage-result publication; that is transitional configuration, not the target engine path. SoFa provides the future admission, supervision, isolation, and operational evidence boundary. OpenCode is the selected external engine for the target and uses the Bastion execution account's OpenAI Codex/ChatGPT authentication. OpenCode/provider configuration selects models per capability. A gateway may route additional providers or add masking, metering, security, and health controls when it supports the selected authentication; SoFa does not become a model server.

## Top-level decomposition

1. **Lighthouse delivery plane.** Repository-local router, workers, routes, stages, gates, gh-aw composition, and stage-result artifacts.
2. **GitHub Actions transport plane.** Event handling, workflow calls, job scheduling, runner capability selection, job status, artifacts, and GitHub delivery evidence.
3. **SoFa runtime plane.** Scheduling, admission, leases, fencing, checkpoints, cancellation, timeout, recovery, supervision, workspace lifecycle, isolation, operational persistence, and health.
4. **Bastion infrastructure plane.** Persistent host, deployed SoFa process, self-hosted runner service, gateway, local models, and execution resources.
5. **Ephemeral execution plane.** One isolated workspace and one SoFa execution worker per run, with bounded concurrent runs on Bastion, connected to the persistent OpenCode engine and its provider configuration.

The dependency direction is deliberate: GitHub Actions invokes Lighthouse and SoFa; SoFa may invoke the engine and observe GitHub evidence; neither SoFa nor OpenCode defines Lighthouse lifecycle semantics.

## Quality realization

| Strategy | Quality property realized |
| --- | --- |
| Router-first dispatch through GitHub Actions | Preserves the existing Lighthouse trigger and authorization boundary. |
| Explicit `ff` composition with fail-stop prerequisites | Prevents stages and gates from being inferred or skipped. |
| Durable lease plus fencing token | Prevents an old worker from publishing after lease loss. |
| Monotonic checkpoints and bounded recovery | Makes interruption diagnosable and resumption explicit. |
| Persistent host plus ephemeral workspace | Keeps operations stable while preventing workspace contamination. |
| Separate evidence categories | Prevents liveness, CI success, or stage completion from becoming human acceptance. |
| Bounded concurrent execution | Fits the single-Bastion constraint while allowing an initial target of five to six isolated runs when health and resource capacity permit. |

## Reference mechanisms and adaptation

Plain Concepts Platform and GitHub Agentic Workflows are mechanism references, not SoFa or Lighthouse product authorities. The reusable patterns are adapted as follows:

| Reference mechanism | SoFa adaptation | Deliberately not copied |
| --- | --- | --- |
| Foundations / harness installation | Treat repository setup and generated workflow assets as Lighthouse-owned consumer-repository installation. | Platform names, lifecycle, policies, and product outcomes. |
| Agentic workflows and event router | Keep the router in Lighthouse and use GitHub Actions as the first dispatch plane. | A Platform-specific router, event taxonomy, or direct agent ingress. |
| Specialized workers and bounded execution | Let Lighthouse workers own stage semantics and SoFa supervise one worker per run up to the configured Bastion capacity. | Platform worker names and assumptions about host topology. |
| `workflow_call` composition and Safe Outputs | Preserve explicit Lighthouse `ff`, artifact validation, and safe output behavior. | New SoFa workflow composition or a new lifecycle stage. |
| Project health and installed versions | Expose SoFa runtime, runner, gateway, model, and Lighthouse compatibility health. | Platform metrics, scoring, or claims that are not defined by Lighthouse or SoFa. |
| OpenCode with Bastion-local Codex authentication | Keep OpenCode external to lifecycle control, authenticate it under the dedicated Bastion execution account, and keep model/provider policy outside Lighthouse. | Moving API keys into GitHub, claiming that the current gh-aw/Copilot worker already runs OpenCode, turning SoFa into a model server, or copying Platform implementation. |

References: [Plain Concepts Platform: AI and SDLC](https://platform.plainconcepts.dev/docs/ai), [Plain Concepts Platform: how we work](https://platform.plainconcepts.dev/docs/how-we-work), [Platform agentic workflows](https://workflows.platform.plainconcepts.dev/), [Platform harness](https://harness.platform.plainconcepts.dev/), and [GitHub Agentic Workflows](https://github.github.com/gh-aw/).
