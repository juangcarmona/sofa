# SoFa

SoFa — my software factory.

SoFa is Juan’s local repository attention application. The first slice reads five repositories and shows current failed checks, PRs awaiting Juan, explicit blockers, and mapped release failures. See [running SoFa](docs/running.md) for Aspire startup and known coverage limits.

## Repository map

- [`docs/product/`](docs/product/README.md): ProductShape model and Product Changes.
- [`docs/architecture/`](docs/architecture/README.md): ArcShape-managed arc42 architecture documentation.
- [`docs/engineering-lifecycle.md`](docs/engineering-lifecycle.md): shared Agentic SDLC gates and working agreements.
- [`src/`](src/README.md): implementation source.
- [`tests/`](tests/README.md): automated tests.

## Tooling

Requires Node.js 22 or newer and the .NET SDK pinned in `global.json`. Install dependencies and run the scaffold checks with:

```sh
npm ci
npm run verify
```

ProductShape and ArcShape are pinned as local development dependencies. Their managed files are refreshed through their CLIs; see [tooling notes](docs/tooling.md).
