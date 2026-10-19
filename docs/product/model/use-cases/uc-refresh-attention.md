---
id: "UC-REFRESH-ATTENTION"
type: "use-case"
title: "Refresh repository attention"
status: "active"
primary-actor: "ACT-JUAN"
supporting-actors: ["ACT-GITHUB"]
bounded-context: "BC-REPOSITORY-ATTENTION"
governed-by: ["BR-OBSERVE-ONLY", "BR-EVIDENCED-ATTENTION"]
uses-terms: ["TERM-ATTENTION-ITEM"]
---

## Goal

Obtain a current attention list and explicit collection outcomes for all five repositories.

## Trigger

Juan requests a refresh.

## Preconditions

The initial repository selection is known. Access may be valid, missing, or incomplete; a missing credential is an observable collection outcome.

## Main Flow

1. Juan requests collection for ArcShape, ProductShape, DomusMind, Raccord, and SoFa.
2. SoFa reads available issue, PR, review, check, and configured release evidence.
3. SoFa classifies findings under BR-EVIDENCED-ATTENTION and reports each repository’s collection outcome, time, and category coverage.
4. SoFa makes the attention list available for UC-REVIEW-ATTENTION, without mutating the sources under BR-OBSERVE-ONLY.

## Alternative Flows

An inspected category with no matching findings is explicitly clear for that observation. Pending checks are described as pending. Release classification without a configured workflow is marked unconfigured.

## Failure Conditions

Authentication failure, insufficient access, rate limiting, or another source failure leaves an explicit reason and partial/unavailable outcome. Evidence obtained for other repositories remains usable. Old observations, if retained, remain visibly dated and are not represented as a successful new refresh.

## Postconditions

Juan can distinguish collected findings from collection gaps for every selected repository. No source work has been changed.
