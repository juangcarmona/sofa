# SoFa

SoFa — my software factory.

SoFa observes and verifies Juan's software factory across repositories. GitHub workflows own orchestration and an independent runtime executes agents. SoFa projects source evidence and highlights decisions or missing information; stopping it does not stop repository delivery.

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

Run `npm run observe -- owner/repository [owner/another-repository]` with an authenticated `gh` client to collect current workflow and issue observations. Its attention list includes requests for information, active agent work, ready items and issues waiting for review, preserving their source labels. This command only reads GitHub. The UI and automatic collection scheduler remain future work.

The former supervisor is retained for historical rollback, disabled by default and no longer started by AppHost. See [ADR-0006](docs/architecture/09-architecture-decisions/0006-observe-repository-owned-workflows.md).
