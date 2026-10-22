---
id: BC-DELIVERY
type: bounded-context
title: Verification and delivery
status: draft
---

## Responsibility

Collect deterministic checks, PR and CI evidence, human decisions, authorized integration outcomes, and verified delivery evidence.

## Language

Verification, evidence, review, authorization, integration, delivery, artifact, and limitation.

## Boundaries

Green automation is not human approval. This context does not own ProductShape intent, architecture decisions, or privileged deployment authority.

## External Relationships

Reads execution outcomes and ACT-GITHUB-BACKLOG evidence, then presents a delivery claim and its limitations to ACT-FACTORY-OWNER.
