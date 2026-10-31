---
id: FR-ISSUE-CONVERSATION
type: functional-requirement
title: Contribute to issue conversations with explicit continuation effects
status: draft
derived-from: [UC-OPERATE-ISSUE, BR-GITHUB-SOURCE, BR-EXPLICIT-WORK-COMMANDS, BR-WORK-AUTHORIZATION]
uses-terms: [TERM-WORK-COMMAND, TERM-WORK-UNIT]
verification:
  - scenario: An authenticated owner posts an ordinary comment, sees its confirmed GitHub identity in the conversation, and no continuation is requested by that action.
  - scenario: A pending question offers an explicit reply-and-continue action only when the repository supports it and the acting user has the required authority.
  - scenario: A lost response while posting a comment remains uncertain and is reconciled without silently posting a duplicate.
---

## Requirement

SoFa MUST let the authorized owner read and publish issue comments attributed to the verified GitHub user. It MUST distinguish an ordinary comment from a reply that requests continuation, describe the latter effect before submission and preserve its association with the pending question and existing authority.

If a repository cannot distinguish these effects, SoFa MUST explain the limitation and MUST NOT present a potentially resuming action as an ordinary comment. Supporting the complete journey requires the shared source contract to express that distinction; SoFa cannot bypass the source's rules. Confirmed comments retain source identity; uncertain publication requires reconciliation before a potentially duplicating retry.

The owner MUST be able to publish ordinary comments while an execution is active. When source policy treats the new comment as invalidating the execution input, SoFa MUST explain that consequence before submission and show the resulting source state. It MUST NOT promise that pending results remain publishable or silently request another execution.

## Rationale

Conversation must remain useful without granting unexpected execution or changing the meaning of a pending human decision.
