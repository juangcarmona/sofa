# SoFa

SoFa — my software factory.

SoFa observes and verifies Juan's software factory across repositories. GitHub workflows own orchestration and an independent runtime executes agents. SoFa projects source evidence and highlights decisions or missing information; stopping it does not stop repository delivery.

## Open the factory view

Run `npm start`, then open `http://127.0.0.1:5168`. Configure the fixed repository list with `SOFA_REPOSITORIES=owner/repo,owner/another-repo`. Use `SOFA_HOST=<trusted-LAN-IP>` to listen on that address and `SOFA_PORT` to change the port. The default list is Lighthouse and SoFa. Keep this read-only surface on the trusted LAN or VPN; it exposes observations for the configured repositories.

The server uses the operator’s existing `gh` authentication, a 15-second shared observation cache and a browser refresh every 30 seconds. GitHub failures appear as unavailable, and refresh failures retain old observations with a stale warning. Workflow actions and logs open their source in GitHub. Embedded privileged controls require a separate authenticated boundary; this version has no mutation API.

Install browser prerequisites with `npx playwright install --with-deps chromium` before running `npm run verify`. CI retains desktop/mobile screenshots and browser reports.

## Repository map

- [`docs/product/`](docs/product/README.md): ProductShape model and Product Changes.
- [`docs/architecture/`](docs/architecture/README.md): ArcShape-managed arc42 architecture documentation.
- [`docs/engineering-lifecycle.md`](docs/engineering-lifecycle.md): shared Agentic SDLC gates and working agreements.
- [`src/`](src/README.md): read-only observations, repository tooling and retained legacy execution code.
- [`tests/`](tests/README.md): runtime, PostgreSQL, container, bridge and repository tests.

## Tooling

Verification requires Node.js 22 or newer, .NET 10 and Docker. Install dependencies and run the checks with:

```sh
npm ci
npm run verify
```

ProductShape and ArcShape are pinned as local development dependencies. Their managed files are refreshed through their CLIs; see [tooling notes](docs/tooling.md).

Run `npm run observe -- owner/repository [owner/another-repository]` with an authenticated `gh` client to collect current workflow and issue observations. Its attention list includes requests for information, active agent work, ready items and issues waiting for review, preserving their source labels. This command only reads GitHub. The web view reuses this collector and exposes source links; it never dispatches work.

The former supervisor is retained for historical rollback, disabled by default and no longer started by AppHost. See [ADR-0006](docs/architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md).

The repository's own development workflows use the pinned Lighthouse gh-aw/OpenCode adapter. See [repository workflow operation](docs/runbooks/repository-workflows.md) for runner setup, stage requests, evidence and recovery.
