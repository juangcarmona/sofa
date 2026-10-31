---
id: ADR-0007
title: Use an external identity provider
status: Accepted
date: "2026-10-26"
deciders: [Juan García Carmona]
---

<!-- markdownlint-disable-next-line MD025 -->
# ADR-0007: Use an external identity provider

## Context

The accepted product model requires authenticated access and a separate GitHub authorization. Identity must remain independent of repository execution.

## Decision

Accepted by Juan García Carmona on 2026-10-31.

Use an external Keycloak instance for authentication. SoFa manages its own application sessions and separately obtains GitHub user authorization. Compose an independent Keycloak instance for development.

## Alternatives considered

- **Embed identity in SoFa:** duplicates identity administration and couples it to the application.
- **Use a managed provider:** viable, but the selected product direction uses shared Keycloak.

## Consequences

- Identity can be shared across applications but becomes an availability dependency.
- Identity credentials and recovery must remain independent of the service being recovered.
- Production provisioning, secret custody and recovery procedures require separate operational design.

## References

- [Accepted product model](../../product/model/use-cases/uc-access-factory.md)
- [Independent repository execution](0006-observe-repository-owned-workflows.md)
