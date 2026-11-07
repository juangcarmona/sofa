---
id: ADR-0009
title: Use .NET and Aspire for SoFa
status: Accepted
date: "2026-10-31"
deciders: [Juan García Carmona]
---

<!-- markdownlint-disable-next-line MD025 -->
# ADR-0009: Use .NET and Aspire for SoFa

## Context

SoFa needs an authenticated application while preserving independent repository delivery. The existing observer uses Node; its Aspire AppHost is currently empty.

## Decision

Use ASP.NET Core for SoFa's application and Aspire to compose its application components and development dependencies. Follow the Lighthouse .NET backend profile. GitHub workflows remain responsible for repository delivery, as stated in ADR-0006.

## Alternatives considered

- **Keep the Node server:** avoids migrating the observer but does not follow the selected .NET stack.

## Consequences

- Existing observation behavior must be preserved while its server adapter is migrated.
- Profile checks, application composition and authentication require implementation and verification.

## References

- [Lighthouse .NET profile](../../profiles/dotnet/README.md)
- [Independent repository execution](0006-observe-repository-owned-workflows.md)
