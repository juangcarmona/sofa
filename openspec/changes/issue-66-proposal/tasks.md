# Tasks

## 1. Clarify the existing verification rule

- [x] 1.1 Add one sentence to `tests/README.md` naming both host-only EngineTests and requiring trusted-host and complete CI passes before integration; verify an exact-content assertion preserves all original text and commands.

## 2. Verify the complete candidate

- [ ] 2.1 Run `npm ci --ignore-scripts`, `npm run verify` and strict OpenSpec validation; retain logs and require passing results before marking this task complete.
- [x] 2.2 Review the final diff for documentation-only scope and retain the check report, PR summary and retrospective under `/output/evidence/`, with unmet gates reported accurately.

Task 2.1 remains incomplete: dependency installation and strict OpenSpec validation passed, but `npm run verify` stopped at the real Gitleaks scanner test (installed binary version mismatch; 26 of 27 Node tests passed). No host tooling was repaired and no checks were weakened. Runtime and browser checks were not reached; see the retained evidence report for independent checks that passed.

## Workflow follow-up

- The trusted host publishes one implementation PR containing this plan and the documentation candidate; this stage does not merge or claim human acceptance.
- The trusted Docker host suite, complete CI suite and owner review remain integration prerequisites, including when local checks cannot complete.
