---
title: Architecture constraints
arc42-section: "02"
description: Technical, organizational, and convention constraints that shape the architecture.
---

# 02. Independent execution

No SoFa database, service, queue or lease is required by a repository workflow. Existing unauthenticated read surfaces remain read-only. Future controls need authentication and use the repository entrypoint. The future UI framework is not selected by this correction.

<!-- pdac:cite id="CON-FACTORY-RUNTIME" digest="sha256:3e183bdd765a663d966e19e3c645ee91e36c8e30b14dacc9f4724c670fd21a2d" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed control boundary

The draft Product Change replaces the loopback-only planning limit with authenticated LAN/VPN access. This boundary must be approved before write routes ship; the current anonymous observer gains no write privilege by configuration drift. The owner selected production Keycloak on node-home and a development Keycloak container inside Aspire. Production provisioning details do not block specification or local development.

The six stages remain triage → refine → propose → implement → integrate → review. Identity, explicit command, reservation, source execution, human gate and published result stay separate. Neither Aspire nor a durable application journal becomes a scheduler. No mandatory terminal, file browser, composite score or public Internet exposure is introduced.
