# Proposal

## Why

The factory owner can observe repository work in SoFa but must leave it to request refinement. Issue [#47](https://github.com/juangcarmona/sofa/issues/47) adds an explicit authenticated request without making SoFa an execution prerequisite or replacing GitHub authority.

## What Changes

- Add a separately authenticated, loopback-only control surface for configured repositories, open-issue selection, review and explicit confirmation of `refine`. Keep the existing LAN observation surface read-only.
- Use external OIDC for SoFa sessions and a separately consented GitHub App user authorization for repository actions; verify the actual user rather than dispatch through an installation token, bot or observation credential.
- Add isolated Aspire-managed development Keycloak initialization, persistent development identities and explicit reset. Production connects to an independently operated configured issuer; SoFa does not manage it.
- Track submission receipts and uncertain/duplicate attempts without an execution queue or automatic retries. Show submission, discovery, source-linked execution and published result as different facts, including ambiguous correlation.
- Add behavioral, security, keyboard/mobile and identity-lifecycle evidence with source links and secret-safe output.

This is a candidate at gate `planned`, not security acceptance or implementation authorization. No breaking change, new stage, automatic continuation or privileged LAN exposure is proposed.

## Capabilities

### New Capabilities

- `authenticated-controls`: isolated session protection, OIDC identity lifecycle and actual-user GitHub authorization for explicit control actions.
- `refinement-requests`: reviewed refinement submission, duplicate receipts, honest source correlation and asynchronous accessible progress.

### Modified Capabilities

None. The checkout has no `openspec/` directory and therefore no existing OpenSpec capability paths. The ProductShape model remains authoritative; these delivery specifications realize, rather than replace, its existing authenticated-adapter permission.

## Impact

Keep `src/web/server.mjs`, its fixed repository observations and operator read credential outside the mutation path. Add a small separate ASP.NET Core control service, session/token storage and a bounded receipt journal; extend `src/SoFa.AppHost/` for development identity orchestration, not execution. Add maintained authentication middleware, selected-repository GitHub App configuration, .NET tests and browser coverage; extend verification/CI and operational documentation. The installed repository router and its actor/concurrency gates remain unchanged. Exact dispatch-to-run correlation is not available from its current inputs; the UI must disclose this limitation rather than infer certainty from the newest run.

Product grounding: `FR-AUTHORIZED-ENTRY`, `BR-WORK-AUTHORIZATION`, `BR-LAN-PRIVILEGE-BOUNDARY`, `CON-TRUSTED-LAN`, `CON-FACTORY-RUNTIME`, and accepted ADR-0006. Identity direction comes from the [verified owner decision](https://github.com/juangcarmona/sofa/issues/47#issuecomment-6419174270); the remaining security design requires separate owner acceptance before implementation. Product semantics changes require a human-approved Product Change.
