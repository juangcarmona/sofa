---
title: Deployment view
arc42-section: "07"
description: Infrastructure and environment mapping for the system.
---

# 07. Deployment separation

Bastion hosts repository-scoped runners and an operator-owned executor service. The runner reaches the executor over a restricted Unix socket. The executor holds engine authentication and starts a bounded container; it has no GitHub publication token or SoFa database. GitHub credentials remain with the workflow publication step. SoFa runs independently with read access to configured sources. AppHost no longer starts the legacy supervisor or PostgreSQL.

<!-- pdac:cite id="CON-FACTORY-RUNTIME" digest="sha256:3e183bdd765a663d966e19e3c645ee91e36c8e30b14dacc9f4724c670fd21a2d" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.
