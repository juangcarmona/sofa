---
id: "FR-ATTENTION-COLLECTION"
type: "functional-requirement"
title: "Collect evidence for the five initial repositories"
status: "draft"
derived-from: ["UC-REFRESH-ATTENTION", "BR-EVIDENCED-ATTENTION", "BR-OBSERVE-ONLY", "CON-EXECUTION-HOST"]
uses-terms: ["TERM-ATTENTION-ITEM"]
verification: [{"scenario": "Given all five repositories are accessible, an on-demand refresh produces a named collection outcome for each and classifies available evidence without writes to GitHub."}, {"scenario": "Given one repository is inaccessible, the refresh reports its reason and retains independently collected outcomes for the remaining repositories."}, {"scenario": "Given a PR has an old failed head and a new head with pending checks, the old failure is not shown as current and the new checks are not reported as successful."}, {"scenario": "Given a configured release target has a failed attempt followed by a successful retry, only the latest attempt determines its current release-failure state."}]
---

## Requirement

The product MUST support an on-demand refresh of exactly the initial selected portfolio:

- `juangcarmona/arcshape`
- `juangcarmona/productshape`
- `juangcarmona/domusmind`
- `juangcarmona/raccord`
- `juangcarmona/sofa`

It MUST collect available evidence for current PR check failures, PRs awaiting Juan’s approval, including his own open non-draft PRs, explicitly blocked issues, and configured failed release runs under BR-EVIDENCED-ATTENTION. Each selected repository and category MUST have an explicit collection outcome. The initial selection is product scope, not a requirement to hard-code repository identities. No new workflows or adoption packages are required in monitored repositories.

## Rationale

The initial value is a trustworthy view of existing work across five repositories, obtainable without a factory engine or new repository instrumentation.
