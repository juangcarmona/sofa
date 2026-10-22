---
id: BC-BACKLOG-TRIAGE
type: bounded-context
title: Backlog and triage
status: draft
---

## Responsibility

Capture ideas, promote them to GitHub issues, reconcile GitHub work into a local coordination view, and shape small work units with priority, scope, acceptance criteria, risk, and dependencies.

## Language

Ideas, backlog items, project priority, work units, dependency edges, readiness, promotion, and triage evidence.

## Boundaries

SoFa may own an idea before promotion, but GitHub becomes the backlog source of truth when the idea becomes or links to an issue. This context does not execute work, approve product intent, or replace repository issue state.

## External Relationships

Reads from ACT-GITHUB-BACKLOG and presents decisions and proposed work units to ACT-FACTORY-OWNER.
