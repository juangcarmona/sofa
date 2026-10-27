# Tasks

## 1. Documentation clarification

- [x] 1.1 Add one sentence naming both host-only EngineTests and requiring the separate host suite and full CI before integration; verify exact test names and that removing the sentence reproduces the original guide byte-for-byte.

## 2. Candidate verification

- [x] 2.1 Run the prescribed dependency, container and strict OpenSpec checks; retain actual results and reconcile the final diff with the documentation-only scope.

Checks were executed, not all passed: dependency installation, Markdown lint, strict OpenSpec validation, ProductShape, architecture and 36 container-scoped runtime tests passed. Container verification failed in a Node security test (Gitleaks version mismatch); standalone security scanning also failed. All four browser tests failed when Chromium crashed before opening a page. Logs and reports are retained under `/output/evidence/`; no successful browser screenshots were produced. Done remains blocked by these failures and the owner's required judgement review; completing task 2.1 records execution, not acceptance.

## Workflow follow-up

- The host publishes one implementation PR containing this plan and the guide change, preserving source, evaluation and reservation-release receipts.
- A trusted Docker host runs the separate engine suite and full CI; the owner reviews all required Done judgements. These gates are not satisfied by a container pass.
- The coordinating operator supplies same-PR feedback, inspects integration controls with `merge=false`, and closes the fixture PR without merging. Do not claim those operations occurred in this engine.
