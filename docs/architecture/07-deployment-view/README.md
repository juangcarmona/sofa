---
title: Deployment view
arc42-section: "07"
description: Infrastructure and environment mapping for the system.
---

# 07. Deployment separation

The read-only SoFa process and the repository runner are independently operated. GitHub publication credentials remain in trusted publication jobs. Runner configuration and deployment details belong to operational configuration.

The web defaults to loopback and may bind a specific trusted-LAN address. Its fixed repository list and GitHub CLI authentication are operator configuration; HTTP clients cannot select arbitrary repositories, run commands or mutate GitHub. No SoFa database or application service participates in workflow execution.

<!-- pdac:cite id="CON-FACTORY-RUNTIME" digest="sha256:3e183bdd765a663d966e19e3c645ee91e36c8e30b14dacc9f4724c670fd21a2d" -->

See [ADR-0006](../09-architecture-decisions/0006-observe-repository-owned-workflows.md) for the accepted boundary and superseded design.

## Proposed environments

| Building block | Development | Production direction |
| --- | --- | --- |
| SoFa web and application store | Existing Aspire solution | Final service packaging is delivery work |
| Keycloak | Dedicated container inside Aspire, test realm/accounts and stable development endpoints | Shared service configured externally |
| Source credentials | Explicit test repository authorization, separate callbacks and secrets | Owner-authorized GitHub connection in protected SoFa storage |
| Agent execution | Independent runner/runtime | Workflow-operated; not a SoFa process |

The source AppHost currently builds an empty application; adding the development Keycloak resource is future implementation. Pin an integration compatible with the installed Aspire version and seed only non-secret test configuration. Development must work without production identity connectivity. Browser clients need reachable HTTPS endpoints for both the application and its selected issuer.

Infrastructure owns production sizing, TLS/DNS, protected storage, backup and restoration in its own operational documentation. These are release-readiness tasks, not prerequisites to writing the product or developing against local Keycloak. [ADR-0007](../09-architecture-decisions/0007-shared-identity-custody-and-recovery.md) retains the proposed custody and environment separation.
