# Tests

`SoFa.Tests/` contains deterministic xUnit tests for rules, refresh coordination, GitHub responses, and the rendered web/antiforgery boundary. Fixtures make no GitHub writes. The synthetic-credential test inspects output, logs and exported traces.

`security/` verifies repository secret-scanner integrity and redaction. Run all checks with `npm run verify` at the root. Runtime evidence belongs under the linked issue’s `docs/delivery/ff/` run.
