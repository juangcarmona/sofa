# Tasks

## 1. Documentation clarification

- [x] 1.1 Add one sentence naming both host-only EngineTests and requiring the separate host suite and full CI before integration; verify exact test names and that removing the sentence reproduces the original guide byte-for-byte.

## 2. Candidate verification

- [x] 2.1 Run the prescribed dependency, container and strict OpenSpec checks; retain actual results and reconcile the final diff with the documentation-only scope.

Initial implementation results (before PR #70 feedback): checks were executed, not all passed. Dependency installation, Markdown lint, strict OpenSpec validation, ProductShape, architecture and 36 container-scoped runtime tests passed. Container verification failed in a Node security test (Gitleaks version mismatch); standalone security scanning also failed. All four browser tests failed when Chromium crashed before opening a page. The initial run retained logs and reports under `/output/evidence/`; no successful browser screenshots were produced in that run. Completing task 2.1 records execution, not acceptance.

## Workflow follow-up

- The host publishes one implementation PR containing this plan and the guide change, preserving source, evaluation and reservation-release receipts.
- A trusted Docker host runs the separate engine suite and full CI; the owner reviews all required Done judgements. These gates are not satisfied by a container pass.
- The coordinating operator supplies same-PR feedback, inspects integration controls with `merge=false`, and closes the fixture PR without merging. Do not claim those operations occurred in this engine.

## 3. PR #70 feedback

- [x] 3.1 Spell out `npm run test:runtime:host` and `npm run verify` in the added sentence; verify both EngineTests names and all other guide wording remain unchanged.
- [x] 3.2 Rerun prescribed checks on the feedback candidate and retain actual results separately from independent host checks; keep owner review pending.

Feedback candidate results: dependency installation and strict OpenSpec validation passed. Container verification passed Markdown lint, then failed with 26/27 Node tests passing because Gitleaks reported a version mismatch; standalone security scanning failed for the same reason. Separately run ProductShape validation/integrity/doctor, architecture validation, all 36 container-scoped runtime tests and all four browser tests passed. The exact documentation comparison and diff whitespace check passed. Current logs, browser screenshots/report and retrospective are retained under `/output/evidence/`. Independent host checks were not performed by this engine. Done remains blocked by security failures and required owner review; host engine tests and full CI remain required before integration. No merge is authorized.
