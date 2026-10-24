# SoFa

SoFa — my software factory.

SoFa is Juan's personal software factory. The ProductShape baseline describes the factory from backlog intake through verified delivery. The first runtime supports supervised triage and refinement with PostgreSQL coordination and isolated OpenCode execution; later delivery stages remain roadmap work.

## Repository map

- [`docs/product/`](docs/product/README.md): ProductShape model and Product Changes.
- [`docs/architecture/`](docs/architecture/README.md): ArcShape-managed arc42 architecture documentation.
- [`docs/engineering-lifecycle.md`](docs/engineering-lifecycle.md): shared Agentic SDLC gates and working agreements.
- [`src/`](src/README.md): runtime, Aspire composition, runner bridge, engine image and repository tooling.
- [`tests/`](tests/README.md): runtime, PostgreSQL, container, bridge and repository tests.

## Tooling

Verification requires Node.js 22 or newer, .NET 10 and Docker. Install dependencies and run the checks with:

```sh
npm ci
npm run verify
```

ProductShape and ArcShape are pinned as local development dependencies. Their managed files are refreshed through their CLIs; see [tooling notes](docs/tooling.md).

For host deployment, provider authorization and live acceptance evidence, follow the [supervised pilot runbook](docs/runbooks/supervised-pilot.md).
