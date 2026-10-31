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

## Proposed environments

| Building block | Development | Production direction |
| --- | --- | --- |
| SoFa web and application store | Existing Aspire solution on Bastion | Recommend node-home; final service packaging is delivery work |
| Keycloak | Dedicated container inside Aspire, test realm/accounts and stable development endpoints | Shared service on node-home, configured externally |
| Source credentials | Explicit test repository authorization, separate callbacks and secrets | Owner-authorized GitHub connection in protected SoFa storage |
| Agent execution | Independent Bastion runner/runtime | Remains workflow-operated on Bastion; not a SoFa process |

The source AppHost currently builds an empty application; adding the development Keycloak resource is future implementation. Pin an integration compatible with the installed Aspire version and seed only non-secret test configuration. Development must work without node-home connectivity. Mobile acceptance needs browser-reachable HTTPS endpoints for both the application and its selected issuer; a loopback callback on the phone is not a Bastion callback.

Infrastructure owns production sizing, TLS/DNS, protected storage, backup and restoration in infra.homelab. These are release-readiness tasks, not prerequisites to writing the product or developing against local Keycloak. [ADR-0007](../09-architecture-decisions/0007-shared-identity-custody-and-recovery.md) retains the proposed custody and environment separation.
