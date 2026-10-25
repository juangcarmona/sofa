---
title: Solution strategy
arc42-section: "04"
description: Fundamental decisions and strategies that shape the architecture.
---

# 04. Project source facts

The first observer is a read-only Node command using the authenticated GitHub CLI. It requests recent workflow runs and open issues for each selected repository, preserves source URLs and timestamps, and marks collection failures unavailable. It creates no queue and performs no writes. This is a bounded observation slice, not a completed web application or delivery verifier.

<!-- pdac:cite id="FR-BACKLOG-PROJECTION" digest="sha256:b98f45997965ba4d187781ca782921eb8c98cb8a9877de2081d26d1e6e5a4d55" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
