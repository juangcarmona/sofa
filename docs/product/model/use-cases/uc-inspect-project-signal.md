---
id: UC-INSPECT-PROJECT-SIGNAL
type: use-case
title: Explain project health and configuration signals
status: draft
primary-actor: ACT-FACTORY-OWNER
supporting-actors: [ACT-GITHUB-BACKLOG]
bounded-context: BC-PORTFOLIO-PROJECT-CONFIGURATION
governed-by: [BR-GITHUB-SOURCE, BR-EXPLAINABLE-SIGNALS, BR-CAPACITY-DEFAULTS]
uses-terms: [TERM-PROJECT, TERM-WORK-UNIT, TERM-DELIVERY-EVIDENCE]
---

## Goal

Decide what to inspect or improve using traceable project signals.

## Trigger

The owner opens a project signal or compares a project over time.

## Preconditions

The owner is authenticated. Configured source access and collection limitations are explicit; incomplete data does not imply an empty factory.

## Main Flow

1. SoFa shows available delivery, harness/workflow adoption, quality and resource signals alongside their availability and observation time.
2. The owner opens a signal to inspect its decision purpose, source records, calculation, time window, coverage and applicable limitations.
3. For a trend or configuration deviation, SoFa identifies the comparison baseline and whether the samples are comparable.
4. The owner follows the source issue, run, check, configuration or artifact, or chooses to shape related work.

## Alternative Flows

Non-applicable delivery metrics are marked not applicable. Incomplete measurements retain their denominator and known sample count. A missing or changed comparison basis prevents an invented trend.

## Failure Conditions

No data is not zero and no telemetry is not healthy. A derived assessment cannot become a repository policy or authorize work.

## Postconditions

The owner can explain the signal, its reliability and the decision it supports, or identify the precise evidence gap.
