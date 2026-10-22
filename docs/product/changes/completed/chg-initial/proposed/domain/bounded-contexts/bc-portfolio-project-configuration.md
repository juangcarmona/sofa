---
id: BC-PORTFOLIO-PROJECT-CONFIGURATION
type: bounded-context
title: Portfolio and project configuration
status: draft
---

## Responsibility

Maintain the configured project repository identities and default branches, and expose discovered project, engineering, workflow, policy, and delivery metadata that the factory uses to coordinate work.

## Language

Project, repository identity, default branch, discovered metadata, profile, capacity, priority policy, risk policy, autonomy policy, and delivery destination.

## Boundaries

This context configures the minimum project references and reconciles discovered metadata, but does not replace GitHub backlog state, execute work, manage agent sessions, or claim delivery. Workspace Management and Agent Runtime remain internal capabilities for now, not separate product contexts.

## External Relationships

Receives configuration decisions from ACT-FACTORY-OWNER and supplies project policy to BC-BACKLOG-TRIAGE, BC-SCHEDULING, BC-EXECUTION, and BC-DELIVERY.
