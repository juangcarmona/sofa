---
title: Deployment view
arc42-section: "07"
description: Infrastructure and environment mapping for the system.
---

# 07. Deployment separation

Bastion hosts the read-only SoFa web process separately from a repository-scoped Actions runner container. The runner launches temporary OpenCode containers through the host Docker socket using operator-owned authentication and image pins. GitHub publication credentials stay in the trusted publication jobs. Only one gh-aw runner is active on this host because gateway startup uses a shared host port; the engine lock alone does not serialize that startup. The previous pilot runner registration and stopped container are retained for recovery.

The web defaults to loopback and may bind a specific trusted-LAN address. Its fixed repository list and GitHub CLI authentication are operator configuration; HTTP clients cannot select arbitrary repositories, run commands or mutate GitHub. No SoFa database or application service participates in workflow execution.

<!-- pdac:cite id="CON-FACTORY-RUNTIME" digest="sha256:3e183bdd765a663d966e19e3c645ee91e36c8e30b14dacc9f4724c670fd21a2d" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
