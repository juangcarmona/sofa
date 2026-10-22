---
id: TERM-EXECUTION-RUN
type: domain-term
title: Execution run
status: draft
defined-in: BC-EXECUTION
synonyms: []
uses-terms: []
---

## Definition

A durable attempt to perform one work unit under a recorded policy, lease, checkpoint sequence, and outcome.

## Distinguish From

A lease grants temporary ownership; a checkpoint records recoverable progress; neither alone is a successful delivery.

## Usage

UC-EXECUTE-WORK-UNIT creates it; UC-MAINTAIN-RUNTIME observes its health; QR-EXECUTION-RECOVERY evaluates its recovery behavior.
