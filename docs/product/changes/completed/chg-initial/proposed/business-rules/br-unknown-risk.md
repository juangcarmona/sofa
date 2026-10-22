---
id: BR-UNKNOWN-RISK
type: business-rule
title: Unknown risk blocks execution authorization
status: draft
applies-to: [BC-BACKLOG-TRIAGE, BC-EXECUTION, BC-SCHEDULING]
uses-terms: [TERM-WORK-UNIT, TERM-EXECUTION-RUN]
---

## Rule

A work unit with unknown or unresolved risk MAY be captured, triaged, and shaped, but MUST NOT be authorized for execution. Deterministic checks and semantic review MAY classify risk. Critical or ambiguous results MUST route to a human gate. When deterministic and semantic assessments disagree, SoFa MUST use the higher risk classification and route the work to a human gate while preserving both assessments. Execution requires a classified risk or an explicit decision that resolves the uncertainty and is recorded with the work unit.

## Rationale

Unknown risk cannot safely select an autonomy policy or establish the boundary for an unattended execution.

## Examples

An issue can be split into a candidate work unit while its security impact is investigated. The scheduler may show it as blocked for risk classification, but it cannot acquire an execution lease for it. A deterministic check may classify a routine documentation change as low risk; an ambiguous security result must go to a human gate. If one assessment says low and the other medium, the work is treated as medium and routed to a human gate.

## Exceptions

None without an explicit recorded risk decision.
