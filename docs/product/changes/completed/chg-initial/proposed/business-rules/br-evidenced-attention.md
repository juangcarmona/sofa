---
id: "BR-EVIDENCED-ATTENTION"
type: "business-rule"
title: "Classify attention from current evidence"
status: "active"
applies-to: ["UC-REFRESH-ATTENTION", "UC-REVIEW-ATTENTION"]
uses-terms: ["TERM-ATTENTION-ITEM"]
---

## Rule

SoFa MUST distinguish findings, pending evidence, unavailable evidence, and an inspected category with no matching findings.

- PR check failures apply to the current head revision or its corresponding current merge revision. An old failed revision is not a current failure. Report the latest attempt of an identified check; a pending attempt does not imply success. Preserve the exact source conclusion rather than treating skipped or cancelled runs as successful.
- An open, non-draft PR awaits Juan’s approval when Juan is its author, when it has an outstanding review request to Juan, or when an explicitly mapped repository convention identifies owner review. Juan’s own PRs qualify without a reviewer request or label and remain listed while open and non-draft, including after other reviewers approve. Draft, closed, and merged PRs do not qualify. Matching several signals produces one approval finding with all applicable reasons. This classification does not constitute a GitHub review or merge authorization. An explicit owner-review label remains a signal until removed.
- A blocked issue is open and explicitly marked by the repository’s mapped blocker convention. Age or inactivity alone does not establish blocking.
- Release failure uses an explicitly identified release or publishing workflow, its newest identifiable release target, and the latest attempt for that target. Older failed targets do not remain on the attention list. A target that cannot be identified reliably produces incomplete coverage. Unrelated CI failure is not a failed release. Missing release-workflow mapping is reported as unconfigured, not clear.
- Each finding names its repository, reason, source identity/link, and evidence time; check findings identify the revision and run. Collection outcomes identify the observation time and coverage. An inaccessible or partially collected category is never reported as clear.

## Rationale

Juan needs evidence he can act on. Guessing from titles, old runs, or missing responses produces misleading attention lists.

## Examples

A failing check on the old PR head is excluded after a new head is pushed; a successful latest retry clears that check’s failure. A repository returning an access error is unavailable even when the other four were read. A release retry supersedes the earlier attempt only for the same identified target.

## Exceptions

None. Unsupported or ambiguous evidence is reported with its limitation.
