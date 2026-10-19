# Running SoFa

## Prerequisites

Bastion was checked on 2026-10-19: Ubuntu 26.04.1, .NET SDK 10.0.112, Aspire CLI 13.6.1. `global.json` pins the SDK. Node.js 22+ runs the repository documentation/security tools. Aspire AppHost and SDK packages are pinned to 13.6.1. AppHost overrides transitive MessagePack to patched 2.5.307; restore treats vulnerability warnings as errors.

## Start locally

From the repository root, restore and build:

```sh
npm ci
npm run verify
```

Provide `Parameters__github_token` to the AppHost from your existing GitHub credential source. For an already authenticated GitHub CLI session, this keeps the token out of shell arguments and files:

```sh
export Parameters__github_token="$(gh auth token)"
aspire run --apphost src/SoFa.AppHost/SoFa.AppHost.csproj
unset Parameters__github_token
```

Do not enable shell tracing or paste token-bearing environment output into logs. No credential is created or changed. Use access to read repository metadata, pull requests, issues, checks/statuses, and Actions for the selected repositories. Access errors are shown as unavailable. Authentication does not grant SoFa permission to write: its adapter uses only GET requests.

Open the SoFa endpoint listed by Aspire. The dashboard link is authenticated and must not be shared. By default both hosts bind to loopback. For trusted LAN access, set `SoFa:LanAddress` in the ignored `src/SoFa.AppHost/appsettings.Local.json` to Bastion’s specific LAN IPv4 address. SoFa then listens directly on that address at port 5168, with an explicit allowed-host entry. The Aspire dashboard remains authenticated and loopback-only. The defaults use loopback HTTP; the LAN override uses HTTP on the configured interface. This mode trusts devices on the LAN to read the attention page and request refreshes; it adds no application login or public ingress. Stop with Ctrl+C. This is a local application session, not an installed boot service.

The page initially says no collection has occurred. **Refresh attention** reads the five repositories. Requests are bounded (two repositories concurrently, 90-second repository budget, 10-second request timeout, ten pages per list, at most one retry for server errors). Overlapping refresh requests share a collection. The result records its collection interval; there is no automatic refresh or history. Restarting discards the snapshot.

For direct web-host diagnosis only, the same credential can be supplied through `GitHub__Token`; `dotnet run --project src/SoFa.Web` uses the same page without Aspire telemetry export unless OTLP is configured.

## What coverage means

- **Complete:** the configured evidence was inspected; findings can still exist.
- **Pending:** source conclusions include pending or other non-success states. Exact conclusions are preserved; skipped/cancelled/neutral are not treated as success.
- **Incomplete:** a limit, changed revision, or ambiguous target prevents a complete assessment.
- **Unavailable:** evidence could not be read (including permissions, rate limits, and timeouts).
- **Unconfigured:** no convention/workflow is mapped. This is not an all-clear.

## Initial repository mappings

The allowlist and explicit mappings live in `src/SoFa.Web/appsettings.json`. Live labels and workflow source were inspected before implementation.

| Repository | Blocker | Owner review | Publishing workflow | Target coverage |
| --- | --- | --- | --- | --- |
| ArcShape | Unconfigured | Unconfigured | `publish.yml` | Verified release tag and run revision |
| ProductShape | Unconfigured | Unconfigured | `release.yml` | Incomplete: execution can check out newer main than run metadata |
| DomusMind | `blocked` | `owner-review` | `release.yml` | Incomplete: dispatch version is not exposed reliably by run metadata |
| Raccord | `status:blocked` | Unconfigured | `release.yml` | Verified tag and run revision; no runs is stated explicitly |
| SoFa | Unconfigured | Unconfigured | Unconfigured | No publishing workflow |

Juan’s authored open non-draft PRs and direct review requests are inspected everywhere, independent of owner-review labels. Approval findings do not mean checks passed or that a PR is mergeable. No repository labels or pipelines are installed by the application.

Release attention considers only the newest target per mapped workflow and its latest attempt. It does not claim coverage of every workflow in the repository. A tag that cannot be verified against the run revision is incomplete. Unsafe/non-GitHub links are rendered as text instead of navigation targets.

## Verification and telemetry

`npm run verify` runs the existing scaffold checks, .NET build, deterministic application tests, and formatting. Tests use synthetic source responses and never write to GitHub. The credential negative test checks page output, captured logs, and exported activities with a distinctive test value.

In Aspire, select `sofa` to inspect `attention.refresh` and `repository.collect` traces, `sofa.collection.duration` and `sofa.collection.outcomes` metrics, and sanitized completion logs. Repository/category/outcome tags are bounded. Bounded failure-reason codes explain incomplete/unavailable category outcomes in traces. Source titles, response bodies, and credentials are excluded from telemetry. The dashboard is operational diagnostics; the page is the attention list.

Bastion was configured for LAN access at Juan’s request on 2026-10-19: `http://192.168.0.54:5168/`. The setting survives Aspire restarts. If DHCP changes the address, update the local file (or reserve that address on the router).
