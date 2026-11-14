# Proposal

<!-- pdac-scope: cited -->

## Why

Operators need to retain an idea before deciding whether it belongs in GitHub. PR #198 must implement the owner-authorized create/link journey without duplicate writes or implicit execution.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->

## What Changes

- Persist owner-scoped drafts with intended outcome, scope and configured repository; allow revisiting and abandonment.
- Inspect related issues, explicitly link an existing issue or review and confirm creation with freshly verified GitHub authority.
- Retain confirmed source identities across refresh and repeated submission; reconcile uncertain creation without resending.
- Keep manual exploration available without a terminal or automatic agent invocation. Existing Lighthouse controls remain the only lifecycle entrypoints after promotion.

## Capabilities

### New Capabilities

- `idea-promotion`: Retained drafts and explicitly confirmed GitHub create/link with uncertain-response reconciliation.

### Modified Capabilities

None. The existing read-only factory overview and GitHub connection contracts remain unchanged.

## Impact

Workspace module, protected ControlJournal storage, authenticated endpoints, native browser assets and additive tests. No new dependency, credential, permission grant, lifecycle stage, scheduler or publication authority.

Full-model comparison: reviewed `docs/product/model`, implementing CHG-LCC-EVOLUTION-001 as already applied. Primary dependencies are FR-IDEA-PROMOTION, UC-TRIAGE-BACKLOG, FR-BACKLOG-PROJECTION, BR-GITHUB-SOURCE and ACT-OPERATOR. ProductShape impact neighbours were checked: BR-WORK-AUTHORIZATION, BR-HUMAN-DECISION-BOUNDARIES, TERM-IDEA and BC-BACKLOG-TRIAGE constrain this slice; scheduling, runtime administration, delivery verification and priority calculation remain unchanged. Evidence: `/output/evidence/product-authority.log`. No accepted model edits or product drift are proposed.
<!-- pdac:cite id="UC-TRIAGE-BACKLOG" digest="sha256:0eaed6ca26d2e957f0eea2fbe207d6b201a0bb25708970f63d4f50c9703db9ac" -->
<!-- pdac:cite id="FR-BACKLOG-PROJECTION" digest="sha256:b98f45997965ba4d187781ca782921eb8c98cb8a9877de2081d26d1e6e5a4d55" -->
<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

Acceptance maps to the six issue bullets: retain drafts; inspect candidates and choose explicitly; verify the current actor and write permission; preserve uncertain source results; keep exploration separate from agent runs; and open the backlog only for a confirmed issue. Fixtures cover create, link, denial, abandonment and lost-response recovery. The connected-user checks were accepted under #52 and do not need to be repeated.
