---
id: "FR-ATTENTION-REVIEW"
type: "functional-requirement"
title: "Present findings with evidence and coverage"
status: "draft"
derived-from: ["UC-REVIEW-ATTENTION", "BR-EVIDENCED-ATTENTION", "BR-OBSERVE-ONLY"]
uses-terms: ["TERM-ATTENTION-ITEM"]
verification: [{"scenario": "Given Juan authored an open non-draft PR with no reviewer request or owner-review label, it appears as awaiting his approval even if another reviewer approved it; multiple matching approval signals produce one finding."}, {"scenario": "Given a current failed check, the list identifies its repository, PR, revision, run, reason, evidence time, and source link."}, {"scenario": "Given an outstanding explicit review request to Juan on an open non-draft PR, the list presents it as awaiting Juan; draft, closed, and merged PRs are excluded from approval findings."}, {"scenario": "Given an open issue explicitly marked blocked by the repository convention, it is listed; an old issue without a blocker signal is not classified as blocked merely because of its age."}, {"scenario": "Given no matching findings but one unavailable category, the list displays both facts rather than reporting that everything is clear."}]
---

## Requirement

The product MUST provide a readable attention list grouped by repository and reason, with source links, evidence times, the collection observation time, and explicit coverage limitations. Juan MUST be able to reach the original evidence. The list MUST distinguish collection problems from repository findings and MUST NOT execute the suggested follow-up.

## Rationale

The first slice succeeds when Juan can make an informed attention decision, regardless of the eventual interface technology.
