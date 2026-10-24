# Tests

`SoFa.Runtime.Tests/` checks admission and fencing against PostgreSQL, kernel peer identity, GitHub authority and receipts, bounded processes, validator diagnostics and container isolation. `bridge.test.mjs` and `engine-cleanup.test.mjs` cover receipt cleanup and engine-session cleanup.

`security/` verifies repository secret-scanner integrity and redaction. Run all checks with `npm run verify` at the root; `npm run test:runtime` provisions an isolated PostgreSQL test container when `SOFA_TEST_DATABASE` is absent. These tests do not replace a live issue-to-stage receipt or owner approval for refinement.
