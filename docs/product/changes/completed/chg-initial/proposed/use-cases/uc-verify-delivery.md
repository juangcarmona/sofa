---
id: UC-VERIFY-DELIVERY
type: use-case
title: Verify work and delivery evidence
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-GITHUB-BACKLOG]
bounded-context: BC-DELIVERY
governed-by: [BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES, BR-LAN-PRIVILEGE-BOUNDARY]
uses-terms: [TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
---

## Goal

Connect checks, PR state, human decisions, integration, and delivered artifacts without overstating evidence.

## Trigger

An execution run reaches verification or a source delivery event changes.

## Preconditions

The run and its source revision are known.

## Main Flow

1. SoFa collects deterministic checks and source-side review/PR evidence.
2. Juan reviews evidence and explicitly authorizes integration when required.
3. SoFa identifies the exact post-integration commit on the default branch, verifies the relevant default-branch pipelines and generated artifacts, and records limitations.

## Alternative Flows

Failed, unavailable, stale, or conflicting evidence stops the claim at the affected boundary.

## Failure Conditions

Green automation, mergeability, or a missing production deployment observation cannot establish delivery acceptance when production verification is in scope. Production deployment is not required for the first pilot.

## Postconditions

The result distinguishes verified facts, human decisions, and unknowns.
