---
title: Solution strategy
arc42-section: "04"
description: Fundamental decisions and strategies that shape the architecture.
---

# 04. Project source facts

The read-only Node collector now serves both the CLI and a native HTML/CSS/JS web interface. The web server uses Node’s HTTP library and Lighthouse UI tokens, keeping the existing collection adapter and adding no application framework. Repository selection is operator configuration. This delivers observation and source navigation; authenticated embedded workflow controls remain future work.

<!-- pdac:cite id="FR-BACKLOG-PROJECTION" digest="sha256:b98f45997965ba4d187781ca782921eb8c98cb8a9877de2081d26d1e6e5a4d55" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
