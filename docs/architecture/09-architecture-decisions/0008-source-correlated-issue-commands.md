---
id: ADR-0008
title: Persist and correlate issue commands
status: Proposed
date: "2026-10-26"
deciders: [Juan García Carmona]
---

<!-- markdownlint-disable-next-line MD025 -->
# ADR-0008: Persist and correlate issue commands

## Context

SoFa needs to relate a user's request to its GitHub result, including when a response is lost. Repository workflows remain responsible for execution.

## Decision

Record command intent and source receipts in the application, and reconcile uncertain requests against source evidence before retrying. Use SQLite for the first single-instance application. Lighthouse and GitHub retain authority over admission, execution and duplicate prevention.

## Alternatives considered

- **Use labels or the latest run as a receipt:** cannot reliably identify a request.
- **Use PostgreSQL:** viable, but adds operational complexity for the proposed single-instance application.

## Consequences

- The application store needs protected persistence and recovery; it is not an execution queue.
- Reliable correlation and duplicate prevention depend on source-side support.

## References

- [Work command product definition](../../product/model/domain/terms/term-work-command.md)
- [Independent repository execution](0006-observe-repository-owned-workflows.md)
- [Shared command contract](https://github.com/juangcarmona/lighthouse/issues/119)
