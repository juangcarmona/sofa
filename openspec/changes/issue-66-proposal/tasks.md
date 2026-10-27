# Tasks

## 1. Clarify the existing verification rule

- [x] 1.1 Add one sentence to `tests/README.md` naming both host-only EngineTests and requiring trusted-host and complete CI passes before integration; verify an exact-content assertion preserves all original text and commands.

## 2. Verify the complete candidate

- [ ] 2.1 Run `npm ci --ignore-scripts`, `npm run verify` and strict OpenSpec validation; retain logs and require passing results before marking this task complete.
- [x] 2.2 Review the final diff for documentation-only scope and retain the check report, PR summary and retrospective under `/output/evidence/`, with unmet gates reported accurately.

Task 2.1 remains incomplete. In the initial implementation, dependency installation and strict OpenSpec validation passed, but `npm run verify` stopped at the real Gitleaks scanner test (installed binary version mismatch; 26 of 27 Node tests passed). In the PR #68 feedback run, dependency installation and documentation lint passed; `npm run verify` stopped with 25 of 27 Node tests passing, with failures in session cleanup and the Gitleaks binary version check. Strict OpenSpec validation could not execute (`openspec: Permission denied`). No host tooling was repaired and no checks were weakened. Runtime and browser checks were not reached; current receipts are retained under `/output/evidence/`.

## 3. Address PR #68 command wording feedback

- [x] 3.1 Expand only the added sentence's two commands to `npm run verify:container` and `npm run test:runtime:host`; verify an exact-content assertion preserves all other README text, both EngineTests names and the integration prerequisite.
- [x] 3.2 Rerun the three required checks and retain current receipts, summary and retrospective; verify the report explicitly preserves any unmet verification gate and leaves task 2.1 incomplete unless all required checks pass.

## Workflow follow-up

- The trusted host updates existing PR #68 with this plan and the documentation candidate; this stage does not open another PR, merge or claim human acceptance.
- The trusted Docker host suite, complete CI suite and owner review remain integration prerequisites, including when local checks cannot complete.
