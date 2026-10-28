# Proposal

## Why

The owner needs to request repository refinement from SoFa on a phone over LAN/VPN and follow the exact source outcome. Issue [#47](https://github.com/juangcarmona/sofa/issues/47) needs an authenticated entry boundary without moving execution or approval into SoFa.

## What Changes

- Add a separate authenticated HTTPS control origin reachable from the owner's phone over LAN/VPN, with reachable OIDC callbacks, protected sessions and CSRF defenses. Preserve the existing anonymous observation service as read-only. Network access grants no authority.
- Use Aspire-managed, persistent, isolated development Keycloak and independently operated external production OIDC, as confirmed by the owner. Separately connect a GitHub App user grant and verify the actual requesting GitHub user.
- Prepare a Product Change replacing the loopback-only planning constraint with this bounded HTTPS option; require independent product, security and architecture acceptance before implementation. The proposed wording is in design.md, not applied product authority.
- Consume Lighthouse [#119](https://github.com/juangcarmona/lighthouse/issues/119) for versioned eligibility, immutable request/payload binding, admission and source reconciliation. Delivery and acceptance of that open proposal are prerequisites, not facts about the installed router.
- Use the documented GitHub dispatch receipt containing `workflow_run_id`, `run_url` and `html_url` for API `2026-03-10`; verify actual GitHub App user-token behavior in implementation. Recover lost responses using source request identity, never latest-run or time matching.
- Keep submitted, exact run discovered, source admitted/running and published result distinct. Add durable receipt recovery, source-enforced duplicate/conflict handling, keyboard/phone UX and real-device acceptance evidence. No automatic redispatch.

This revises [PR #60](https://github.com/juangcarmona/sofa/pull/60) only. `planned` means a reviewable candidate, not product/security approval, implementation permission or merge authorization.

## Capabilities

### New Capabilities

- `authenticated-controls`: separately protected remote control access, OIDC lifecycle and actual-user GitHub authorization.
- `refinement-requests`: explicit source-authorized refine requests with exact request/run/result attribution and recoverable uncertainty.

### Modified Capabilities

None. `/workspace/openspec` is absent; the supplied candidate's two capability paths are retained. ProductShape remains authoritative and its accepted model is not changed by these delivery specifications.

## Impact

Propose a small server-rendered ASP.NET Core service under `src/SoFa.Control/`, separate from `src/web/server.mjs` and its observation credentials. Extend `src/SoFa.AppHost/` only for control development and isolated identity orchestration. Add authentication middleware, protected session/token storage, a non-secret receipt journal, deployment documentation and .NET/browser checks in verification/CI.

The current router at Lighthouse pin `63c64b779ef18959a9c993902a5e5e1225a16947` has no immutable request identifier. A future reviewed installer upgrade must supply the accepted #119 contract; do not hand-edit generated workflows or invent its wire interface. GitHub direct entry remains supported and repository-owned. No queue, runtime, scheduler, policy engine, later-stage action or approval is added.

Authority: `FR-AUTHORIZED-ENTRY`, `CON-TRUSTED-LAN`, `BR-LAN-PRIVILEGE-BOUNDARY`, `BR-WORK-AUTHORIZATION`, `CON-FACTORY-RUNTIME`, ADR-0006 and the [verified identity decision](https://github.com/juangcarmona/sofa/issues/47#issuecomment-6419174270). The [phone/correlation feedback](https://github.com/juangcarmona/sofa/pull/60#issuecomment-6429159957) and [supplied upstream evidence](https://github.com/juangcarmona/sofa/pull/60#issuecomment-6429302951) authorize this revision, not acceptance.
