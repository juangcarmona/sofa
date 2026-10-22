---
id: FR-DELIVERY-EVIDENCE
type: functional-requirement
title: Verify delivery from source evidence
status: draft
derived-from: [UC-VERIFY-DELIVERY, BR-GITHUB-SOURCE, BR-HUMAN-DECISION-BOUNDARIES]
uses-terms: [TERM-EXECUTION-RUN, TERM-DELIVERY-EVIDENCE]
verification: [{scenario: "Given a run, PR, CI result, authorized integration decision, and default-branch target, SoFa records the exact integrated commit, relevant pipeline runs, generated artifact identities, timestamps, and human approval separately."}, {scenario: "Given missing or stale delivery evidence, SoFa reports the limitation rather than claiming successful delivery."}]
---

## Requirement

The product MUST link local checks, PR state, review state, CI, integration authorization, the exact post-integration commit on the default branch, relevant pipeline runs, generated artifact identities, and post-integration verification. It MUST keep human decisions distinct from automation and report unknown, unavailable, or stale delivery evidence. The first pilot MUST NOT require production deployment verification.

## Rationale

Evidence is the basis for trustworthy delivery claims.
