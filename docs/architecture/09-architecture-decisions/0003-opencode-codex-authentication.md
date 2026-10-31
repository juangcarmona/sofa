---
id: ADR-0003
title: Authenticate OpenCode outside the workflow
status: Accepted
date: "2026-10-24"
deciders: [Juan García Carmona]
---

<!-- markdownlint-disable-next-line MD025 -->
# ADR-0003: Authenticate OpenCode outside the workflow

## Context

Repository workflows need an execution engine without making the workflow responsible for the model provider's credentials.

## Decision

OpenCode uses the operator's Codex authentication in the independently operated execution environment. Provider credentials remain outside GitHub workflow inputs and the repository workspace. The original SoFa supervision arrangement was superseded by [ADR-0006](0006-observe-repository-owned-workflows.md).

## Alternatives considered

- **Put a provider API key in the workflow:** moves provider authentication into the repository workflow.
- **Make SoFa a model server:** duplicates provider routing and expands its responsibility.

## Consequences

- The execution environment owns the protection and renewal of the provider credential.
- Authentication must be verified in the actual non-interactive execution context.

## References

- [Independent repository execution](0006-observe-repository-owned-workflows.md)
- [OpenCode providers](https://opencode.ai/docs/providers)
