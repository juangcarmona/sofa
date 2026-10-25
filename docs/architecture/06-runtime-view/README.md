---
title: Runtime view
arc42-section: "06"
description: Runtime behavior and important interactions between building blocks.
---

# 06. Repository execution and observation

An authorized GitHub event reaches the repository router and one stage worker. The worker reserves the issue, collects current source and owner answers, invokes the independent executor, validates the result, rechecks authority and publishes through GitHub. Named Actions steps, progress metadata, a summary and artifacts expose the run. SoFa may refresh observations at any time, including after the run; its availability does not affect execution. An owner answer to a pending refinement resumes through the same repository route.

<!-- pdac:cite id="FR-RECOVERABLE-EXECUTION" digest="sha256:02460c262c7ad33b35456a43d2f878a3791494aae51aa1b70f2578069054f176" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
