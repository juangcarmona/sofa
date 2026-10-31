---
id: UC-EXPLORE-FACTORY
type: use-case
title: Find work across repositories and project views
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-GITHUB-BACKLOG]
bounded-context: BC-PORTFOLIO-PROJECT-CONFIGURATION
governed-by: [BR-GITHUB-SOURCE, BR-PRIORITY-DEFAULTS]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
---

## Goal

Find the relevant work without opening each repository separately.

## Trigger

The owner opens the factory or selects a project.

## Preconditions

The owner is authenticated. Configured source access and collection limitations are explicit; incomplete data does not imply an empty factory.

## Main Flow

1. SoFa presents configured repositories, their source access, collection scope, active work and outstanding intervention.
2. The owner searches by issue title or identity and filters by repository, issue state, lifecycle stage, human wait and execution state.
3. The owner switches between an issue list and a project board, retaining selection and explicit filter scope.
4. Opening a project shows its backlog, current work, evidence, configuration and health signals; opening a card enters the same issue operation journey.

## Alternative Flows

The owner can open an issue directly without an attention label. Board grouping is explained and represents source facts; multiple waits or unknown stage remain visible. Selecting or grouping work never requests a transition.

## Failure Conditions

A failed repository read is shown separately from zero results. Incomplete pagination and filtered scope are explicit. No hidden repository is inferred accessible from another source.

## Postconditions

The owner can reach the intended issue with its source identity and understand what the current view includes.
