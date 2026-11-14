# Tasks

<!-- pdac-scope: cited -->

## 1. Retained draft and source operations

- [x] 1.1 Add owner-scoped drafts to ControlJournal and implement review/create/link/reconcile with revision checks; verify create, link, denied permission, abandonment, lost response, restart and concurrency in additive .NET tests.

## 2. Operator journey

- [x] 2.1 Add the authenticated Ideas screen with draft revisit, related issues, exact preview, explicit confirmation and source receipt; verify browser interactions, keyboard access, light/dark desktop/narrow layouts and retain screenshots. Document the operation and its limitations.

## 3. Candidate verification

- [x] 3.1 Run the configured checks, strict OpenSpec and citation verification; retain terminal receipts and retrospective.
<!-- pdac:cite id="FR-IDEA-PROMOTION" digest="sha256:02726ca033f3c323ee9b2898ea36086cdebe41aa270c6d785ececc5c1912b9e3" -->
<!-- pdac:cite id="BR-GITHUB-SOURCE" digest="sha256:fd8141424d5fead38ed55d54689511fc445467133706e875371cc3199af8ab1a" -->

## Workflow follow-up

- Trusted host independently verifies the candidate and unchanged PR #198 head/discussion before updating that same branch.
- The connected-user checks in #52 are accepted and do not need to be repeated for this increment.
- Required forge CI, independent review and authorized integration remain separate. Do not publish, merge or archive from this worker.
