---
id: "TERM-ATTENTION-ITEM"
type: "domain-term"
title: "Attention item"
status: "draft"
defined-in: "BC-REPOSITORY-ATTENTION"
synonyms: []
uses-terms: []
---

## Definition

An observation tied to a repository and source item that explains a current failed check, PR awaiting Juan’s approval, explicit blocker, or failed release run requiring investigation. It carries the reason, source identity, evidence time, and source link.

## Distinguish From

A collection problem means SoFa could not establish the repository state; it is not a failed repository check. An attention item is neither a new work item nor an authorization to act.

## Usage

UC-REFRESH-ATTENTION collects and classifies evidence; UC-REVIEW-ATTENTION lets Juan inspect the resulting items.
