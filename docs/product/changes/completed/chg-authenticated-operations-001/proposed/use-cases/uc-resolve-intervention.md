---
id: UC-RESOLVE-INTERVENTION
type: use-case
title: Resolve a source-backed request for human intervention
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-GITHUB-BACKLOG]
bounded-context: BC-BACKLOG-TRIAGE
governed-by: [BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES, BR-EXPLICIT-WORK-COMMANDS]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
---

## Goal

Understand and resolve the decisions or information that currently need the owner.

## Trigger

The owner opens the intervention inbox or a work item enters a human wait.

## Preconditions

The owner is authenticated. Configured source access and collection limitations are explicit; incomplete data does not imply an empty factory.

## Main Flow

1. SoFa lists source-backed requests for information, proposal review, approval, or operator action with the reason, age and related issue or run.
2. The owner opens the request and reads the actual question, evidence and responsible actor, including why action is needed now.
3. SoFa offers only supported source-authorized actions, distinguishing ordinary comments from reply-and-continue and protected approvals.
4. After an explicit action, SoFa follows its receipt and refreshed source state before showing the request resolved.

## Alternative Flows

A stale or already-resolved request is refreshed and explained. Several requests for one issue keep their separate identities and contexts. Source policy may resolve a wait without a SoFa action.

## Failure Conditions

Unknown ownership or unavailable evidence is shown as such. An ordinary comment, visit or failed write cannot clear a wait. Unsupported approval actions explain their source fallback.

## Postconditions

The request is source-confirmed resolved, still waiting, rejected, or uncertain with an explanation; no gate was bypassed.
