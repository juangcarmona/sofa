---
id: "UC-REVIEW-ATTENTION"
type: "use-case"
title: "Review attention and reach its evidence"
status: "draft"
primary-actor: "ACT-JUAN"
supporting-actors: []
bounded-context: "BC-REPOSITORY-ATTENTION"
governed-by: ["BR-OBSERVE-ONLY", "BR-EVIDENCED-ATTENTION"]
uses-terms: ["TERM-ATTENTION-ITEM"]
---

## Goal

Understand what deserves attention and reach the evidence needed to act in the source tools.

## Trigger

Juan opens or requests the collected attention list.

## Preconditions

A collection result is available, including a result containing only collection problems.

## Main Flow

1. Juan reads the list grouped by repository and attention reason, together with its observation time and collection coverage.
2. Juan inspects an item’s explanation and source link.
3. Juan follows the link to the source tool to investigate or act; SoFa does not perform the action.

## Alternative Flows

If no findings match, the list still shows the inspected scope and any pending, unconfigured, partial, or unavailable categories. Presentation may be through a terminal, report, or other small interface; no web application is required by this use case.

## Failure Conditions

A link may become inaccessible or the source may change after collection. The observation time makes that limitation visible; Juan can request another refresh.

## Postconditions

Juan knows what was observed, what remains unknown, and where to investigate. No product or merge approval has been inferred.
