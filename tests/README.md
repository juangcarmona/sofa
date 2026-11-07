# Tests

`web.test.mjs` checks the observation server's fixed repository scope, cache, failure handling and read-only HTTP boundary. `browser/` exercises source links, unavailable and stale observations, keyboard navigation, mobile layout and accessibility in both themes. Run `npx playwright install --with-deps chromium` once before `npm run test:browser`; screenshots and reports are written under ignored `test-results/` and `test-reports/` directories and retained by CI.

`SoFa.Runtime.Tests/` checks admission and fencing against PostgreSQL, kernel peer identity, GitHub authority and receipts, bounded processes, validator diagnostics and container isolation. `bridge.test.mjs` and `engine-cleanup.test.mjs` cover receipt cleanup and engine-session cleanup.

`security/` verifies repository secret-scanner integrity and redaction. Run all checks with `npm run verify` at the root; `npm run test:runtime` provisions an isolated PostgreSQL test container when `SOFA_TEST_DATABASE` is absent. These tests do not replace a live issue-to-stage receipt or owner approval for refinement.

For an isolated .NET container with host-provisioned `SOFA_TEST_DATABASE`, run `npm run verify:container`. It runs the complete application verification, including PostgreSQL and browser tests, and names the excluded Docker engine suite. Run that suite on a trusted Docker host with `npm run test:runtime:host`. Both suites are required for container acceptance; a container pass alone is not full verification. `npm run verify` and CI continue to run every test by default.

`SoFa.Web.Tests/` exercises maintained OIDC/cookie middleware with signed synthetic issuer responses, owner/session deadlines, concurrent provider refresh, source-account callback/permission isolation, per-session observation caches and actual HTTP asset bodies/MIME types. Composition tests prove external mode provisions no provider and development uses only the workspace/provider plus SDK tooling.

`development-identity.test.mjs` checks that setup keeps secrets outside the checkout with private permissions, generates valid certificate chains and refuses replacement of an existing setup. `npm run verify:dotnet` adds Release/format/module checks and real LH1001/LH1002 compiler rejection to the provisioned solution-test lanes.

`npm run verify:identity:host` requires Docker, Chromium, OpenSSL and `certutil`. It builds the exact Release AppHost, provisions an isolated private CA/browser and real pinned Keycloak, then tests actual MFA, fresh reauthentication, refresh, revocation, outage, accessibility and local cleanup. It takes about twelve minutes because provider revalidation uses the real five-minute interval. Source UI states use explicit browser fixtures; real GitHub consent and an actual phone are separate acceptance. Only safe screenshots and summary receipts are retained. Existing `browser/` tests invoke the legacy loopback fixture explicitly so `npm start` remains the authenticated application command.
