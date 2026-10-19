---
id: "BR-OBSERVE-ONLY"
type: "business-rule"
title: "Observation preserves human authority"
status: "active"
applies-to: ["BC-REPOSITORY-ATTENTION"]
uses-terms: ["TERM-ATTENTION-ITEM"]
---

## Rule

This slice MUST only read repository evidence and present attention items. It MUST NOT change repository content, issues, labels, reviews, workflow runs, product decisions, or merge state, or launch agents. Product decisions and merges remain Juan’s decisions.

## Rationale

The immediate value is an attention list. Future execution requires a separately approved product change and must retain the stated human approval boundaries.

## Examples

A failed PR check links to its evidence; collecting it neither reruns the workflow nor dispatches a repair agent. A requested review remains pending until Juan acts in the source tool.

## Exceptions

None in this slice.
